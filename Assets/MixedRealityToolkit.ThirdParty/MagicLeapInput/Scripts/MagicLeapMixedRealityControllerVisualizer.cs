using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HoloLab.MixedReality.Toolkit.MagicLeapInput
{
    public class MagicLeapMixedRealityControllerVisualizer : MixedRealityControllerVisualizer
    {
        [SerializeField]
        bool showModel = false;

        protected override void Start()
        {
            base.Start();
            foreach (Transform model in transform)
            {
                model.gameObject.SetActive(showModel);
            }
        }
    }
}
