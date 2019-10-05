# CS561-HalmaEditor

An editor for editing boards of Halma game.

## Download

[Google Drive](https://drive.google.com/open?id=1fk8teay6F7fJIZLwGEOB2oT7On7UnP0d)

## Start Instruction

1. Unzip folder
1. Edit `appsettings.json`, change the value of default input.txt file path in the settings `"BoardManager": "FilePath": "<your default input path>"`
1. Run `HalmaEditor.exe` or other executable according to your operating system
1. Wait a second and look at the second line of the output on the prompt.
    Open the URL with your favorate broswer. The URL should be like `http://localhost:5000`

## Editor Usage

### File Operation

* Click `Open` to open a input file, whose path can be changed.
* Once a input file is opened, the editor will monitor the file changes and update automatically.
* To open a new editor to track different input.txt file by clicking `Open New Editor` on the left.
* To untrack the file and open a new file, reenter a path and click `Open`.
* To untrack the file without opening a new file, just close the broswer tab.
* To save as a new input file, just reenter a path and click `Save as New`.

### Board Operation

* Click `Single/Game` button to change mode.
* Click `Black/White` button to change color.
* Change the value of input `Left Time` to change the time value.
* Click __white or black__ square to change brush color, __cyan__ square to choose eraser.
* Click any tile on the broad to toggle the tile with current brush.
