using UnityEngine;

namespace UntitledRPG.Actor
{
    public class FollowTransform : MonoBehaviour
    {
        [SerializeField] private Transform _followTransform;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private bool _followX = true;
        [SerializeField] private bool _followY = true;
        [SerializeField] private bool _followZ = true;

        void OnValidate()
        {
            UpdatePosition();
        }

        void FixedUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition() => transform.position = new Vector3(
                _followX ? _followTransform.position.x + _offset.x : transform.position.x,
                _followY ? _followTransform.position.y + _offset.y : transform.position.y,
                _followZ ? _followTransform.position.z + _offset.z : transform.position.z);
    }
}
