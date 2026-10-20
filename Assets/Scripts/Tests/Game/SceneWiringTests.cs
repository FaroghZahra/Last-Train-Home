using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Game.Bootstrap;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LastTrainHome.Game.Tests
{
    /// <summary>
    /// Catches scene mistakes before Play mode does: unassigned references, missing scripts, and buttons wired in the
    /// Inspector instead of in code.
    /// </summary>
    public sealed class SceneWiringTests
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        private Scene _scene;
        private bool _openedByTest;

        [OneTimeSetUp]
        public void OpenScene()
        {
            _scene = SceneManager.GetSceneByPath(ScenePath);
            if (!_scene.isLoaded)
            {
                _scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                _openedByTest = true;
            }
        }

        [OneTimeTearDown]
        public void CloseScene()
        {
            if (_openedByTest)
            {
                EditorSceneManager.CloseScene(_scene, removeScene: true);
            }
        }

        private IEnumerable<T> All<T>() where T : Component =>
            _scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(includeInactive: true));

        [Test]
        public void Scene_HasExactlyOneBootstrap()
        {
            Assert.That(All<GameBootstrap>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ProjectComponents_HaveEverySerializedReferenceAssigned()
        {
            var unassigned = new List<string>();
            foreach (MonoBehaviour component in All<MonoBehaviour>().Where(IsProjectComponent))
            {
                SerializedProperty property = new SerializedObject(component).GetIterator();
                while (property.NextVisible(enterChildren: true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.name != "m_Script"
                        && property.objectReferenceValue == null)
                    {
                        unassigned.Add($"{PathOf(component.transform)} > {component.GetType().Name}.{property.name}");
                    }
                }
            }

            Assert.That(unassigned, Is.Empty);
        }

        [Test]
        public void GameObjects_HaveNoMissingScripts()
        {
            string[] broken = All<Transform>()
                .Where(transform => transform.GetComponents<Component>().Any(component => component == null))
                .Select(PathOf)
                .ToArray();

            Assert.That(broken, Is.Empty);
        }

        [Test]
        public void Buttons_HaveNoListenersAssignedInTheInspector()
        {
            string[] wiredInInspector = All<Button>()
                .Where(button => button.onClick.GetPersistentEventCount() > 0)
                .Select(button => PathOf(button.transform))
                .ToArray();

            Assert.That(wiredInInspector, Is.Empty, "Hook buttons up in code (onClick.AddListener in the view).");
        }

        private static bool IsProjectComponent(MonoBehaviour component) =>
            component != null && component.GetType().Namespace?.StartsWith("LastTrainHome") == true;

        private static string PathOf(Transform transform) =>
            transform.parent == null ? transform.name : $"{PathOf(transform.parent)}/{transform.name}";
    }
}
