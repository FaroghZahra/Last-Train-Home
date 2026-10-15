using LastTrainHome.Game.Flow.Views;
using TMPro;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>One stop in the planning screen's route list: where, who gets off, and when.</summary>
    [DisallowMultipleComponent]
    public sealed class StopRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _time;

        public void Show(PlanStopModel stop)
        {
            _label.SetText(stop.Label);
            _time.SetText(stop.Time);
        }
    }
}
