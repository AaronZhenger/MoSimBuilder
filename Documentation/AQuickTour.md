## Familiarizing

* Once the project is open, you will see something similar to the image below.
  <img width="1911" height="1014" alt="image" src="https://github.com/user-attachments/assets/21ef0829-ee9f-4389-bef0-9626776f66ae" />
* On the bottom of the screen (as shown in the below image) is our `Project Window`. This is where files are
  stored.
  !<img width="1916" height="278" alt="image" src="https://github.com/user-attachments/assets/cb16f84f-87b3-4f1e-9241-4db3db7b00b4" />
* on the left side you see the `folder list`, this is for quick access to specific folders.
* In the middle is the `file explorer` where you can see and modify files by clicking on them
* Now, scroll down the `file eplorer` and then click the `scenes` folder to open the scenes folder
  <img width="1714" height="246" alt="image" src="https://github.com/user-attachments/assets/01eef288-cfbd-44de-868a-020415d764d8" />
* In the center of the above photo is the `File Viewer`. Double click the file named `Field`.
* This filetype is known as a `Scene`. Scenes are playable zones, and thus where we will return to when running robots.
* double click the `Field Scene` to load the play scene for builder.
  <img width="1915" height="680" alt="image" src="https://github.com/user-attachments/assets/1294733f-6072-477d-ae14-5776675d7e7a" />
* Looking back up to the center of the screen, we can see the `Hierarchy` located on the left, and the `Scene/Game View`
  located in the middle with the `Play Button` right above the `Scene View` with the `Inspector` to the right.
 <img width="225" height="634" alt="image" src="https://github.com/user-attachments/assets/905f71b0-b01b-481e-a285-40ba63e530a4" />
* In the Hierarchy we see a few things: `GameManager`, `Directional Light`, `SpawnPoint`, and `GameUi`.
    * The `GameManager` is where the field and robots are set
    * The `Directional light` is the light shone on the field
    * `SpawnPoint` is the position that the robots will spawn in at
    * `GameUi` manages the UI and other static elements
<img width="1271" height="714" alt="image" src="https://github.com/user-attachments/assets/84ba2c5c-acde-476c-b679-5db58c29da58" />
* The `Scene/Game View` is where the work is actually done
* The `Play`, `Pause` and `Step` buttons located above the `Scene View` are what spawns the robots and initializes
  gameplay
  <img width="421" height="679" alt="image" src="https://github.com/user-attachments/assets/354f3001-c7d3-45e7-9b2c-1254dfa4e993" />
* This brings us to the final tab, the `Inspector` window. When you select an object using either `Scene View` or the
  `Hierarchy` this window on the right side of the screen will populate with the "components" of the object.
    * When you select the `GameManager` object in the `Heirarchy` (left click) the `Inspector` menu will populate with
      its components, as shown below:
      <img width="421" height="678" alt="image" src="https://github.com/user-attachments/assets/d522c830-7788-44bd-81d2-03302ca403b5" />
    * This is where the bulk of the changes will occur.
    * The first "Component" of interest is the `LoadMatch` script. In it we have:
        * The field prefab to be used. To select a field, just drag it to the top of the list
        * The spawn point to use. This is defaulted to the premade `SpawnPoint` from the field scene
        * The robot selector. Each robot from the `Resources -> Robots` folder will be automatically added to the
          dropdown list
        * The view type
<img width="1917" height="247" alt="image" src="https://github.com/user-attachments/assets/d50bb41d-9052-4cd4-89a5-337ec167fa99" />
* Finally we return to the `Project View` and open the `Resources` folder, then the `Robots` folder. This is where all
  the robots are located.
  (Photo does not necessarily include all robots)
    * There is no requirement that the names of the robots be numbers, however they can not be `duplicates`.
    * All robots are automatically added to the robot selector list in the `GameManager`
    * To play, select the desired robot and click the `Play` button above the `Scene View`.
    * Controls? Whatever you make them.
 
  # [First Robot](FirstRobot.md)
