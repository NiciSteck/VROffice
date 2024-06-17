# VROffice - Automating Physical Alignment of Mixed Reality Workspaces
VROffice is a Unity Project for my Bachelor Thesis at ETH Zurich. It combines [MRMapper](https://github.com/yv1es/MRMapper) and elements from [InteractionAdapt](https://dl.acm.org/doi/abs/10.1145/3586183.3606717) to automatically build environments out of the surfaces in the users physical surroundings. The environments can be used to build a Mixed Reality Workspace based on the affordances of the real world. For more details check out the [thesis](https://github.com/eth-siplab-students/t-bt-2023-MREnvironment-NicolasSteck/blob/master/thesis/Bachelor_thesis_Nicolas_Steck.pdf).

*Parts of this README are taken directly from the [MRMapper README](https://github.com/yv1es/MRMapper/blob/master/README.md). There you can find more detailed information regarding MRMapper*

The project includes:
- automatic definition and serialization of environments
- automatic recognition and alignment of saved environments
- features from InteractionAdapt
  - manual definition and calibration of environments
  - adaptive handtracking controllers
  - a selection of virtual elements
    - browser
    - keyboard
    - buttons
- features from MRMapper
    - surface detection using an external camera
    - tracking of the real world
      - pointcloud
      - camera position *(not functional because of drift over time)*

## Installation
Clone this repository to your machine.

### Prerequisites
To use the features of VROffice your system needs to meet the following requirements:
- Windows operating system
- [Docker](https://www.docker.com/) installed
- [Unity](https://unity.com) installed
- [Python 3](https://www.python.org/) with [virtualenv](https://packaging.python.org/en/latest/guides/installing-using-pip-and-virtual-environments/) installed

### Unity Project
The unity project is located inside the [VROffice-Unity](https://github.com/NiciSteck/VROffice/tree/main/VROffice) folder. You can set it up using Unity Hub (Open -> Add project from disk).

### MRMapper

**Docker:**
To set up the core functionality of MRMapper, follow these steps:

1. Open a terminal and navigate to the `MRMapper/core` folder.
2. Execute the provided batch script: `build_image.bat` to build the Docker image named `mrmapper-image`. Please note that the image will require approximately 13 GB of disk space, and the build process can take up to an hour.

**Python:**
The camera control scripts, responsible for managing the RealSense camera, run directly on the host system. The `findOrientation.py` script to align environments is also located here. To set up the necessary environment and install dependencies, follow these steps:

1. Open a PowerShell window and navigate to the `MRMapper/camera` folder.
2. Create a new virtual environment named "env" by executing the following command: `python -m venv env`.
3. Activate the virtual environment with the command: `.\env\Scripts\activate`. Note: If you encounter an error related to script execution policy, run the following command in an administrator PowerShell to allow execution of locally created scripts: `set-executionpolicy remotesigned`.
4. Install the required dependencies inside the virtual environment using the command: `pip install -r requirements.txt`.

## Execution

VROffice consists of three main components:

### MRMapper Core
To start the MRMapper core, navigate to `MRMapper/core` and execute the `run.bat` script. This will launch a Docker container with MRMapper's core functionality.
`run.bat` will also launch a terminal running Flask which provides the Unity project with an API to call the `findOrientation.py` script.
In case you only need to restart the MRMapper core you can use `runMROnly.bat`.

### Unity
Open the DevScene under `Asset/Scenes` in the project explorer and launch the play view. 
After playing the Unity scene, it will connect to the core running in the container so make sure that the container is up an running first.

**DevScene:**
Includes all the functionality but no user interface. The different functions can be triggered in the inspector through the script components attached to the `AdaptiveInput/PhysicalEnvrionmentManager/Environments` object.

**AutomaticStudy Scene:**
Provides a small user interface to define or calibrate environments automatically. The + button defines a new environment once you confirm. The folder button calibrates the environment once you confirm. MRMapper needs to be started manually.

**ManualStudy Scene:**
Provides a small user interface to define or calibrate environments manually. The + button defines a new environment once you confirm. The folder button calibrates the environment once you confirm. 

### Python Environment
The system has been tested with an [Intel RealSense D435i](https://www.intelrealsense.com/depth-camera-d435i/) camera connected via a USB 3.0 cable.

To run the camera scripts, please ensure that the virtual environment set up during the installation is active in your current terminal session. If you are using PowerShell, "(env)" should appear to the left of "PS". If not, activate the virtual environment by executing the script `/MRMapper/camera/env/Scripts/activate`. If you are using VSCode you can also use the `Python Environment Manager` extension.

In the `MRMapper/camera` folder, you will find several Python scripts:

* `stream.py`: Start a live stream from the camera to MRMapper for real-time operation.
* `record.py`: Record footage with the camera to a file on disk. Use the "-h" flag for help on how to use this script.
* `playback.py`: View recorded footage and replay it for MRMapper. This is useful for testing and development. Use the "-h" flag for help on how to use this script.
* `groundtruth.py`: Determine the position of ArUco markers in the scene relative to the current camera position. This is useful for ground truth evaluation. Use the "-h" flag for help on how to use this script.
* `findOrientation.py` Handles the automatic alignment and exposes a REST API usinf Flask. It should already be up and running through `run.bat`.

### Summary
To make use of the automatic features of this project, follow these steps:

1. activate your python environment with `/MRMapper/camera/env/Scripts/activate`.
2. Launch the MRMapper core and the Flask Server with `MRMapper/core/run.bat`.
3. Start Unity and click play to enter the play view.
3. Connect the RealSense camera and start streaming the camera feed with `MRMapper/camera/stream.py`.

You should now see the point cloud and reconstructed planes in Unity. Now you can use the scrips attached to the `AdaptiveInput/PhysicalEnvrionmentManager/Environments` object.

**Definition:**
Once you see all captured surfaces displayed in unity you can convert the surfaces into an environment by setting the `build` boolean on the `BuildEnv` script.

**Calibration:**
Once you see all captured surfaces displayed in unity you can calibrate an environment by setting the `calibrate` boolean on the `CalibrateEnv` script.
*The `recenter` boolean just realigns the active environment instead of going through all saved environments.*
