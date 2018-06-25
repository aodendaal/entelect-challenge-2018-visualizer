using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookUserInput : MonoBehaviour
{
    private bool freeLookActive;

    // Use this for initialization
    private void Start()
    {
        CinemachineCore.GetInputAxis = GetInputAxis;
    }

    private void Update()
    {
        freeLookActive = Input.GetMouseButton(0); // 0 = left mouse btn or 1 = right
    }

    private float GetInputAxis(string axisName)
    {
        return !freeLookActive ? 0 : Input.GetAxis(axisName == "Mouse Y" ? "Mouse Y" : "Mouse X");
    }
}
