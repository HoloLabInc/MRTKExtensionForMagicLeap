using System.Collections;
using UnityEngine;

namespace HoloLab.MixedReality.Toolkit.MagicLeapInput
{
    public class PlaceRelativeToStartPosition : MonoBehaviour
    {
        private void Awake()
        {
            StartCoroutine(UpdatePositionCoroutine());
        }

        private IEnumerator UpdatePositionCoroutine()
        {
            // Wait untile Camera transform is updated
#if UNITY_EDITOR
            // Editor & MLRemote
            yield return new WaitForSeconds(0.3f);
#elif PLATFORM_LUMIN
            // Magic Leap
            yield return null;
#endif
            UpdatePosition();

            yield return null;
        }

        private void UpdatePosition()
        {
            var cameraTransform = Camera.main.transform;
            var objectPosition = cameraTransform.TransformPoint(transform.position);

            // Use angle around Y axis
            var forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();
            var cameraRotation = Quaternion.LookRotation(forward);
            var objectRotation = cameraRotation * transform.rotation;

            transform.SetPositionAndRotation(objectPosition, objectRotation);
        }
    }
}
