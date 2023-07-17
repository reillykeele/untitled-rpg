using ReiBrary.Helpers;
using UnityEngine;

namespace UntitledRPG.Effects.Environment
{
    public class WindController : MonoBehaviour
    {
        public Vector3 WindDirection = Vector3.zero;
        public Vector3 WindArea = Vector3.one;

        private ParticleSystem[] _systems;

        void Awake()
        {
            _systems = GetComponentsInChildren<ParticleSystem>();
        }

        void OnDrawGizmosSelected()
        {
            GizmoDrawHelper.DrawBox(transform.position, WindArea / 2, Quaternion.Euler(WindDirection), Color.cyan);
        }

        void OnValidate()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();

            foreach (var system in systems)
            {
                system.transform.rotation = Quaternion.Euler(WindDirection);

                var shape = system.shape;
                shape.scale = WindArea;
            }
        }
    }
}
