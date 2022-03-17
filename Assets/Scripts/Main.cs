/*
 * KnownBugs: 
 * 1.Could be that if you smash the undo button to fast something messes up.
 * 2.Might be some inconsitencies with the camera when clicking buttons.
 * 
 * I could fix but ran out of time.
 * 
 * Some Instructions I didnt have time to implement such as the camera zoom.
 * And the game ending screen (just use the menu button to start a new game) 
 * I would have liked to implement them but just not really any more time to work on this.
 * 
 * The Main entry point for the entire application.
 * Here we setup a basic Monobehavior for everything else to kick off from.
 *
 *First I would like to say I would never structure a real application this way however  
 *given the time restraints and simplicity of this assignment this is probably the optimal way
 *to structure it.  There is only ever one cube and it never rotates.  Just the camera.
 *Only thing that moves are the "cubes/faces" inside the rubics and they do so in a repeatable manner.
 *
 *Usually there are lots of things moving around and lots of moving pieces in games and in those cases.
 *I would structure things very differently.  
 *
 *I am using Structs over classes for performance and simplicity.
 *DOnt need to check if null able to reasonably understand the memory layout and simplier than 
 *having references everywhwere.  
 *
 *The large amount of static classes here just stems from the fact there is no need for instantion of objects
 *since there is only one of everything in a rubix cube game for the most part.
 *
 *We do use many gameobject but these are reasonbly self contained in large arrays.
 *
 *The nature of this app is not indicative of what an actual game usually does. 
 *
 *Because every entity / cube in the rubics cube is basically static most of the time and 
 *the structure and positions are relatively stable frame to frame.
 *
 *My Cube Generation here can support WAYYY more than 2x2 / 6x6 just fills up the camera so for now
 *I set it to maximum 6x6.
 *Cube Face Generation code is very sloppy given the time constraints I started doing something than realized its
 *not very optimal but not much time so I just followed through with it.
 *
 *Overall I have seen the easy way to do something like this would be to do alot of raycasting onto 
 *a prebuilt cube.  And use the results of the raycasts to find the location of the faces
 *results what cubes to rotate etc....
 *
 *Issues with that most of the time Raycasts can be very expensive in real large data sets.
 *So here to show an alteranttive way I used only one RayCast(ScreenCast) to find the face of interest
 *and the rest is done using just basic 3d math and mouse position.
 *Cube state is kept by a single 3d array (two if you count the one I use to check against for completion).
 *Rotation is done by calculating the slice of interest than doing some SetParent to make an easy rotation
 *cooridinate system.  (Not happy with that for SetParent() perf is not great) But it works for a quick and dirty
 * way.  I do a NxN matrix Transpose to rotate the 3d arrays slices in place.
 * 
 * Things I would change if I had the time would be to ...
 * 1.Remove static class from Cube so you could have multiple rubix cube instatiated.
 * 2.Set the cube one level deeper and use local instead of world cooridiates for some of the calculations
 *      So we rotate the cube if we wanted and still have things work as is.
 * 3. I would usually  pay alot more attention to Garbage Collection issues which I have here I just did
 *      the easy thing and didnt worry about Garbage Collection also given the nature of this game (not very real time intensive)
 *      I just left it up in the air.
 * 4.I feel I didnt have really as much time to work on making things as nice as I would have liked 
 *      since there were a lot of minonr details to implement.
 * 5.My Rubics Cube is very ugly but no Assets were included so I just used procedural style generation.
 *      Would be nice if there were some assets included that I could have linked with. to make it prettier perhaps.
 *      
 *      There are many other things to say about this but I will stop here.  Thank you for reading this if you took the time.
 *      Finally there are comments littered about as requested to describe in minor detail how things are working in 
 *      certain key areas.
 * 6. There are some magic numbers laying around but again im running out of time here so just letting you know im 
 *      aware they are there and generally should not.
 **/


using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct EntityID
{
    public int id;
}

public class Main : MonoBehaviour
{
    //title UI Connections
    public GameObject UIParent;

    public Button NewGameButton;
    public Button ContinueGameButton;

    public Slider CubeDimSlider;
    public bool NewGameStart;
    public TextMeshProUGUI SliderCount;

    //game ui connections
    public GameObject GameUIParent;
    public TextMeshProUGUI TimerText;
    public Button MenuButton;
    public Button UndoButton;
    public TextMeshProUGUI GameOverText;

    //init
    void Start()
    {
        
        //InitUI Calls itself from a monobehavior scene script GameUI.cs
        //And since we can only create the cube after the user has selected 
        //a size the main init code resides there.
        //Cube.InitCube(); 
    }

    void Update()
    {
        Game.UpdateGame(TimerText);
    }
}
