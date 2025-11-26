# Getting Started

## Downloading

* Unity is roughly 5.21 GB
* MoSimBuilder is roughly 1.65 GB
* These are estimates and untested

### The first step is to download Unity Hub

<h5>(Unity Hub is the application that is used to edit Unity applications, such as MoSimBuilder)</h5>

* [Download Here](https://unity.com/download)
* Once installed open the app.
* Use a personal liscence
* DO NOT install editor versions during setup

### The Second Step is to download the MoSimBuilder source. This can be done in a couple ways

* <h4>Simple install</h4>
    * Install from the main MoSimBuilder GitHub page using Code -> Download Zip
    * Go to your downloads folder and unzip to a location
* <h4>_advanced install_</h4>
  (optionally fork first)
    * Open the GitHub Desktop app
    * File -> Clone Repository -> fill out fields

### The final Step is to let Unity Hub install the correct version of Unity for you

* In the unity hub app click the `Add` button in the top right
  <img width="772" height="582" alt="image" src="https://github.com/user-attachments/assets/770bfcd8-a7a9-49fd-877f-ed4e6ccb9a3e" />
* Next select the project from your disk
* Double click your project folder to open the outer layer, then select the folder with the name MoSimBuilder -
  V0.0.....
* If you selected properly is will ask you if you want to install the correct version of Unity, click yes.
* It will then ask you ask you about adding modules, the default is all that you need checked.
  ![image](https://github.com/user-attachments/assets/cad4705a-0795-4613-ba4c-2c0d5f1c7224)
* you can now open the project once the download is complete. NOTE: The editor takes a long time to download.

### Updating

* If you used the advanced install simply fetch from , then pull the origin on the GitHub Desktop app
* If you used the simple install you will need to start from scratch.

## Getting Started

### Familiarizing

* Once the project is open, you will see something similar to the image below.
  <img width="1911" height="1014" alt="image" src="https://github.com/user-attachments/assets/26b95c99-6aba-4704-86a2-31255e5ba393" />
* On the bottom of the screen (as shown in the below image) is our `Project Window`. This is where files and scenes are
  stored.
  ![c43656f0-f2d7-4133-ae5d-ecb374d28579](https://github.com/user-attachments/assets/666f7452-7d31-4656-8ca6-d95f3a99b3ac)
* On the left side of the `Project Window` is the `File Browser`. Scroll and then double click the `scenes` folder to
  open the file location in the image.
* In the center of the above photo is the `File Viewer`. Double click the file named `Field`.
* This filetype is known as a `Scene`. The scene is where all the magic happens. All of the individual components are
  used by the scene to vizualize the designs.
* Looking back up to the center of the screen, we can see the `Hierarchy` located on the left, and the `Scene/Game View`
  located in the middle with the `Play Button` right above the `Scene View`.
  ![4eb4c310-33f1-4662-9b2c-350a3a5cac3f](https://github.com/user-attachments/assets/f023d61d-2e1f-44bb-9a21-2bd49921e62f)
* In the Hierarchy we see a few things: `GameManager`, `Directional Light`, `SpawnPoint`, and `GameUi`.
    * The `GameManager` is where the field and robots are set
    * The `Directional light` is the light shone on the field
    * `SpawnPoint` is the position that the robots will spawn in at
    * `GameUi` manages the UI and other static elements
* The `Scene/Game View` is where the work is actually done
* The `Play`, `Pause` and `Step` buttons located above the `Scene View` are what spawns the robots and initializes
  gameplay
* This brings us to the final tab, the `Inspector` window. When you select an object using either `Scene View` or the
  `Hierarchy` this window on the right side of the screen will populate with the "components" of the object.
    * When you select the `GameManager` object in the `Heirarchy` (left click) the `Inspector` menu will populate with
      its components, as shown below:
      ![f35c028a-07b7-4fee-ae88-200d59513959](https://github.com/user-attachments/assets/8b3ea1da-bfae-4a9b-ab5e-cb95bacafb5d)
    * This is where the bulk of the changes will occur.
    * The first "Component" of interest is the `LoadMatch` script. In it we have:
        * The field prefab to be used. To select a field, just drag it to the top of the list
        * The spawn point to use. This is defaulted to the premade `SpawnPoint` from the field scene
        * The robot selector. Each robot from the `Resources -> Robots` folder will be automatically added to the
          dropdown list
        * The view type
* Finally we return to the `Project View` and open the `Resources` folder, then the `Robots` folder. This is where all
  the robots are located.
  ![90746f51-7cff-4cb2-bb4e-3bd8c9a5f2f9](https://github.com/user-attachments/assets/6d3a6089-86a3-4efd-a17d-ba0c6aa215a7)
  (Photo does not necessarily include all robots)
    * There is no requirement that the names of the robots be numbers.
    * All robots are automatically added to the robot selector list in the `GameManager`
    * To play, select the desired robot and click the `Play` button above the `Scene View`.
    * Controls? Whatever you make them.

## Controls Disclaimer

* The controls are made for Xbox Controllers
* Each action can be set to a keyboard and a controller button
* Swerve actions cannot be altered per-robot
  * `WASD`/`Left Stick` - Translation
  * `J/L`/`Right Stick X-/+` - Rotation

# [First Robot](https://github.com/masonmm3/MoSimBuilder/blob/Stable/Documentation/FirstRobot.md)

## Blue text in large font indicates a link to the next step in the documentation. Click the blue words to continue learning about Builders inner workings
