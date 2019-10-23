# CS561-HalmaEditor

A board editor for Halma game.

Please leave a star if you find it helpful :blush:

* Automatically update input.txt and editor bidirectionally.
* Automatically apply output.txt.
* Automatically run the game using your program.
* Support multi-file monitoring.
* Developed with Blazor(.net core 3.0) from Microsoft.
* Welcome to leave issues and pull requests.

![demo](https://drive.google.com/uc?id=17KCGxVDV2CPpe1U3Bv-_8g9O0Yc_quQG)

## Download

[Click here to download](https://github.com/panyz522/CS561-HalmaEditor/releases/)

_Available for Windows 10, Mac, and Linux_

## Getting Started

1. Unzip folder
1. Edit `appsettings.json`, change the default paths of input.txt and output.txt in the settings.

    `"BoardManager": "FilePath": "<your default input path>"`

    `"BoardManager": "OutputFilePath": "<your default output path>"`

1. Run `HalmaEditor.exe`(Windows) or `HalmaEditor`(MacOS & Linux) according to your operating system. 
    
    **NOTE:** MacOS may need permissions. Please try `sudo spctl --master-disable` to disable [Gatekeeper](https://en.wikipedia.org/wiki/Gatekeeper_(macOS)).
    
1. Wait a second and look for a line starting with **Now listening on**.
    Open the URL with your favorate broswer. Or just open [http://localhost:5000](http://localhost:5000)

## Editor Usage

### File Operations

* Click `Link input.txt` to open a input file, whose path can be changed.
* Once a input file is opened, the editor will monitor the file changes and update automatically.
* To open a new editor to track different input.txt file, click `Open New Editor` on the Nav Panel.
* To untrack the file and open a new file, reenter a path and click `Link input.txt`.
* To untrack the file without opening a new file, just close the broswer tab.
* To save as a new input file, just reenter a path and click `Save As`.
* To monitor and automatically apply the output.txt to the current board, enter the **Output File Path** and click `Link output.txt`.
* To manually apply the output.txt, click `Apply output.txt`

### Board Operations

* Click `Single/Game` button to change mode.
* Click `Black/White` button to change color.
* Change the value of input `Left Time` to change the time value.
* Click __white or black__ square to change brush color, __cyan__ square to choose eraser.
* Click any tile on the broad to toggle the tile with current brush.

### Runner Operations

* To open a new runner, click `Open New Runner` on the left Nav Panel.
* Enter the **Command** and **Working Directory** and click `Run` to manually run your program.
* Choose one of the linked boards to bind to that board, so when the board is updated, the program runner can be automatically triggered.
    * The chosen board must bind to an input.txt and an output.txt to make runner work.
