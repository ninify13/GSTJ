using System;
using UnityEngine;

namespace Core.InputManager
{
    public class InputManager : MonoBehaviour
    {
        public Action<Vector3> OnMouseDown = default;
        public Action<Vector3> OnMouseHold = default;
        public Action<Vector3> OnMouseUp = default;

        public Vector3 MousePosition { get { return Input.mousePosition; } }

        void Awake()
        {

        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnMouseDown?.Invoke(Input.mousePosition);
            }
            else
            if (Input.GetMouseButton(0))
            {
                OnMouseHold?.Invoke(Input.mousePosition);
            }
            else
            if (Input.GetMouseButtonUp(0))
            {
                OnMouseUp?.Invoke(Input.mousePosition);
            }
        }
    }
}