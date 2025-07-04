using UnityEngine;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    public CinemachineFreeLook camMotorcycle;
    public CinemachineFreeLook camAirplane;
    public CinemachineFreeLook camBoat;

    public GameObject Motorcycle;
    public GameObject Airplane;
    public GameObject Boat;

    void Start()
    {
        SetActiveCamera(camMotorcycle, Motorcycle);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetActiveCamera(camMotorcycle, Motorcycle);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetActiveCamera(camAirplane, Airplane);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetActiveCamera(camBoat, Boat);
    }

    void SetActiveCamera(CinemachineFreeLook activeCam, GameObject vehicle)
    {
        camMotorcycle.Priority = 0;
        camAirplane.Priority = 0;
        camBoat.Priority = 0;

        activeCam.Priority = 10;

        MotorcycleController motorcycleController = Motorcycle.GetComponent<MotorcycleController>();
        AirplaneController airplaneController = Airplane.GetComponent<AirplaneController>();
        BoatController boatController = Boat.GetComponent<BoatController>();

        if (motorcycleController != null)
            motorcycleController.isActive = false;
        if (airplaneController != null)
            airplaneController.isActive = false;
        if (boatController != null)
            boatController.isActive = false;

        MotorcycleController selectedMotor = vehicle.GetComponent<MotorcycleController>();
        if (selectedMotor != null)
            selectedMotor.isActive = true;

        AirplaneController selectedPlane = vehicle.GetComponent<AirplaneController>();
        if (selectedPlane != null)
            selectedPlane.isActive = true;

        BoatController selectedBoat = vehicle.GetComponent<BoatController>();
        if (selectedBoat != null)
            selectedBoat.isActive = true;

    }

}