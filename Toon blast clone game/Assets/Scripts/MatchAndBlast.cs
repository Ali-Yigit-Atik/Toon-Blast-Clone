using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MatchAndBlast : MonoBehaviour
{

    // Important: Board layout is created with the help of editor script. When you change the number of rows and columns,
    // wait for the board layout in the editor to update for a few seconds before starting the game. Otherwise, you may encounter bugs.
    // If you have a bug, the bug will be gone when you start the game again.

    //In addition, to avoid misunderstanding, I want to clarify that  there are no errors or bugs in the script and codes. The bug only
    //appears when the game is started before the board layout updates itself.


    [Header("Board Layout can control which array element is open or close(empty or not)")]
    public ArrayLayout boardLayout; // It will be used to make the array elements we want empty and to shape the game board.
    [Header("")]
    [Header("When you change row or column values please wait few seconds to board layout uptated itself to avoid bug")]
    public int row;
    public int column;
    private int width; // will be used to derive the algorithm more easily.
    private int height; // will be used to derive the algorithm more easily.
    public RectTransform gameBoard; // parent objcet of color blocks

    public static GameObject[,] colors; //The array that holds the active colored blocks in the game.
    public GameObject[] colorobjects; // color blocks prefabs
    public GameObject shuffleText; // when there is a no match this text show up then color blocks gonna shuffle.

    [Header("Enter the changing intervals of the sprites according to the number of matches")]
    public int defaultIconToFirstIconCount; // prefabs images change according to the their match size
    public int firstIconToSecondIconCount;  
    public int secondIconToThirdIconCount;

    [Header("Level Goal Values")]
    public TextMeshProUGUI MovementText;  // show how much movement left
    public TextMeshProUGUI SuccessAndGameOverText; // after success or failure this text show up
    public int blastNumberOfEachColorBlocks;   // number of each color that must be blasted for task
    public int maxMovementCount; // total movement number that player has
    private int redBlockBlasted;  // how much colorful block blasted
    private int purpleBlockBlasted;
    private int blueBlockBlasted;
    private int greenBlockBlasted;
    private int yellowBlockBlasted;
    private int pinkBlockBlasted;
    private int totalBlockShouldBlasted; // number of the block that needed to blast to complate task
    private int currentBlockBlasted;  

    
    private bool needColors; // will be used to create a new object order when the game starts or  there is no match
    private bool checkMatches; // will be used for match checking.
    private bool needNewColorBlocks; // is used after blast
    private bool needShuffle = false; // is used for shuffle control
    private bool isGameOver = false; // It is used to stop the game when the task is successful or unsuccessful.

    
    private List<GameObject> objectPool = new List<GameObject>(); // using for object pooling (for beter performance)
    

    

    private AudioSource audio_;

    private void Awake()
    {
        width = column; // will be used to derive the algorithm more easily.
        height = row; // will be used to derive the algorithm more easily.
        totalBlockShouldBlasted = colorobjects.Length * blastNumberOfEachColorBlocks;
        SuccessAndGameOverText.gameObject.SetActive(false);
        audio_ = gameObject.GetComponent<AudioSource>();

        for(int i=0; i <= (row * column) + 50; i++) // create object pool (I add extra color blocks to avoid some bugs)
        {
            GameObject colorBlock = Instantiate(colorobjects[Random.Range(0, colorobjects.Length)], gameBoard);
            colorBlock.SetActive(false);
            objectPool.Add(colorBlock);
        }
    }
    void Start()
    {
        //bool conditions are assigned that will affect the functions contained in the update.
        colors = new GameObject[width, height];
        needColors = true;
        checkMatches = false;
        needNewColorBlocks = false;        
        shuffleText.SetActive(false);
        
        
    }

    
    void Update()
    {
        if(width == column && height == row) // this condition is assigned to avoid possible bugs
        {
            InstantiateObject(needColors); // is run when the game starts or when shuffle is needed
            checkifMatch(checkMatches); // it works after the game starts and after blast. it also runs the function that changes the icons of the color blocks
            blast(ColorBlocks.isTriyingBlast); //works when the player clicks on any block
            spawnNewColors(needNewColorBlocks);// it works after blast

        }

        if(maxMovementCount>=0) MovementText.text = maxMovementCount.ToString(); //writes the remaining number of moves to the screen
        if (maxMovementCount <=0 || currentBlockBlasted>= totalBlockShouldBlasted) SuccessAndGameOver(); // runs when the task succeeds or fails


        if (objectPool.Count < 20) //Sometimes, when a large number of blocks are blasted rapidly and in rapid succession, some blasted objects
                                   //may not enter the pool again.  This for loop is built to avoid any instance errors                                   
        {
            for (int i = 0; i <=10; i++) // create object pool 
            {
                GameObject colorBlock = Instantiate(colorobjects[Random.Range(0, colorobjects.Length)], gameBoard);
                colorBlock.SetActive(false);
                objectPool.Add(colorBlock);
            }
        }
        
    }

    void InstantiateObject(bool isNeedObjects) // Instantiate Object when game starts or there is no any match
    {
        if (isNeedObjects)
        {
            needColors = false;
            

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++) 
                {


                    if (colors[x, y] != null) // if array is not empty pass gameobject to object pool
                    {
                        colors[x, y].gameObject.SetActive(false);
                        objectPool.Add(colors[x, y].gameObject);
                        colors[x, y] = null;

                    }

                    bool isneedFill = (boardLayout.rows[y].row[x]) ? false : true;  // Fill array element if object is not marked as empty in editor


                    if (isneedFill)
                    {

                        //Generate a random integer to get an object from the object pool. 
                        //Assign array element with the help of that integer and object pool
                        //Place assigned object on parent object
                        //Remove the object from the object pool after the object is assigned

                        int randomIndex = Random.Range(0, objectPool.Count);  
                        objectPool[randomIndex].GetComponent<RectTransform>().anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * y));
                        
                        colors[x, y] = objectPool[randomIndex];
                        colors[x, y].gameObject.SetActive(true);
                        objectPool.RemoveAt(randomIndex);

                        colors[x, y].gameObject.name = colors[x, y].gameObject.tag + "[" + y.ToString() + ", " + x.ToString() + "]";

                    }



                }
            }
            needColors = false; // stop assigned objects
            checkMatches = true; // check if there is match
        }
        

    }


    private List<GameObject> checkNeighborsColors(int x, int y) // This function is used inside the checkifMatch and blast functions.
    {

        //This function finds whether the color block with the given array indexes is in any match. And it creates a match list.

        //The working logic of the function: The color block's neighbors on the right(1,0), left(-1,0), top(0,1), and bottom(0,-1) sides are
        //checked, if it has the same color as itself, it adds it to the list. Meanwhile, the list is inside a for loop.
        //Neighbors of each new list member are checked in the same way and neighbors of the same color are added to the list.
        //Color comparison is done with the help of tags. The tag of each color block specifies its color.


        List<GameObject> colorMathes = new List<GameObject>();

        if (!colorMathes.Contains(colors[x, y]))
        {
            colorMathes.Add(colors[x, y]);

            for (int i = 0; i < colorMathes.Count; i++)
            {
                int x_ = 0;
                int y_ = 0;


                for (int k = 0; k < width; k++)
                {
                    for (int z = 0; z < height; z++)
                    {

                        if (colors[k, z] == null || colors[k, z].gameObject.activeSelf == false) // Skip the places where the object will not be placed
                        {
                            continue;
                        }
                        else if (colors[k, z].gameObject.name == colorMathes[i].gameObject.name)
                        {
                            x_ = k;
                            y_ = z;
                        }
                    }
                }

                if (!(x_ < 0 || x_ >= width - 1))
                {
                    int xx = (x_ + 1);
                    int yy = y_;

                    if (colors[xx, yy] != null && colors[x_, y_].gameObject.tag == colors[xx, yy].gameObject.tag)
                    {


                        if (!colorMathes.Contains(colors[xx, yy]))
                        {
                            colorMathes.Add(colors[xx, yy]);
                        }

                    }
                }

                if (!(x_ <= 0 || x_ > width - 1))
                {
                    int xx = (x_ - 1);
                    int yy = y_;

                    if (colors[xx, yy] != null && colors[x_, y_].gameObject.tag == colors[xx, yy].gameObject.tag)
                    {

                        if (!colorMathes.Contains(colors[xx, yy]))
                        {
                            colorMathes.Add(colors[xx, yy]);
                        }

                    }
                }

                if (!(y_ < 0 || y_ >= height - 1))
                {
                    int xx = x_;
                    int yy = (y_ + 1);

                    if (colors[xx, yy] != null && colors[x_, y_].gameObject.tag == colors[xx, yy].gameObject.tag)
                    {

                        if (!colorMathes.Contains(colors[xx, yy]))
                        {
                            colorMathes.Add(colors[xx, yy]);
                        }

                    }
                }

                if (!(y_ <= 0 || y_ > height - 1))
                {

                    int xx = x_;
                    int yy = y_ - 1;



                    if (colors[xx, yy] != null && colors[x_, y_].gameObject.tag == colors[xx, yy].gameObject.tag)
                    {

                        if (!colorMathes.Contains(colors[xx, yy]))
                        {
                            colorMathes.Add(colors[xx, yy]);
                        }

                    }
                }

            }



        }


        return colorMathes;
    }


    public void checkifMatch(bool needCheckMatches) // It will be used to control the matches and change the icons of the color
                                                    // blocks according to the match numbers.
    {


        //The working logic of the function: With the help of checkNeighborsColors, the matches of all array elements are found
        //and the sprite assignment of the color blocks is made according to the number of the match it is in. It also checks
        //the total number of matches on the game board and runs the shuffle function if there are no matches.


        int totalMatchOnTheGameBoard = 0; // the value is created to  check if there is a need to shuffle.

        if (needCheckMatches )
        {
            

            List<GameObject> matchList = new List<GameObject>(); // created to keep the matches

            for (int x = 0; x < width; x++)
            {


                for (int y = 0; y < height; y++)
                {
                    matchList.Clear();

                    if (colors[x, y] == null || colors[x, y].gameObject.activeSelf == false) // Skip the places where the object will not be placed
                    {
                        continue;
                    }
                    else
                    {
                        matchList = checkNeighborsColors(x, y);
                    }

                    if (matchList.Count >= 2) // define how many blastable groups there are if there are matching colors
                    {
                        totalMatchOnTheGameBoard++;
                    }

                    foreach (var color in matchList) // Change icons of matched color blocks according to the number of groups
                    {
                        changeColorSprite(color, matchList.Count);
                        
                    }


                }
            }

            
            if (totalMatchOnTheGameBoard == 0) // its means shuffling block colors
            {

                needShuffle = true;                
                shuffleText.gameObject.SetActive(true);
                Invoke(nameof(shuffleColorCubes), 2f);
                
            }

        }


        checkMatches = false; // quit check matches

    }


    public void changeColorSprite(GameObject color, int matchCount) // Change icons of matched color blocks according to the number of groups
    {
        if (matchCount < defaultIconToFirstIconCount)
        {
            color.GetComponent<Image>().sprite = color.GetComponent<ColorBlocks>().colorSprites[0];
        }
        if (matchCount >= defaultIconToFirstIconCount && matchCount < firstIconToSecondIconCount)
        {
            color.GetComponent<Image>().sprite = color.GetComponent<ColorBlocks>().colorSprites[1];
        }
        if (matchCount >= firstIconToSecondIconCount && matchCount < secondIconToThirdIconCount)
        {
            color.GetComponent<Image>().sprite = color.GetComponent<ColorBlocks>().colorSprites[2];
        }
        if (matchCount >= secondIconToThirdIconCount)
        {
            color.GetComponent<Image>().sprite = color.GetComponent<ColorBlocks>().colorSprites[3];
        }
    }



    public void blast(bool checkBlast)  //Check if there is a match. Blast if 2 or more color blocks of the same color are adjacent
    {

        List<GameObject> matchList = new List<GameObject>();


        if (checkBlast && isGameOver ==false)
        {

            int blastObejectX = 0;
            int blastObejectY = 0;

            //The working logic of the function: The array index of the clicked object is determined and its neighbors are checked to see
            //if it is the same color as itself. If there is an object that matches it, it writes it to the matchlist and
            //starts a loop through its members in the matchlist. It checks the neighbors of newly joined members to see if
            //there are color blocks of the same color. After all members are checked, the color blocks in the matchlist are blasted.

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    if (colors[x, y] == null || colors[x, y].gameObject.activeSelf == false) // Skip the places where the object will not be placed
                    {
                        continue;
                    }
                    else if (colors[x, y].gameObject.name == ColorBlocks.gameObjectName)
                    {
                        blastObejectX = x;
                        blastObejectY = y;
                    }
                }
            }



            matchList = checkNeighborsColors(blastObejectX, blastObejectY);


        }


        if (matchList.Count >= 2)
        {

            //If there are 2 or more matching color blocks of the same color, they are taken into the for loop and blasted.

            //The colors of the matched objects are confirmed for the task given for the level and how many of which colors
            //the color block was blasted is written. If the correct number is reached for the given task, the counting is stopped.


            audio_.Play(); // play when there is a blast.
            for (int i = 0; i < matchList.Count; i++)
            {
                                
                matchList[i].gameObject.SetActive(false);

                if (matchList[i].gameObject.CompareTag("blue") && blueBlockBlasted <=blastNumberOfEachColorBlocks)
                {
                    blueBlockBlasted++;
                    currentBlockBlasted++;
                    
                }
                else if (matchList[i].gameObject.CompareTag("red") && redBlockBlasted <= blastNumberOfEachColorBlocks)
                {
                    redBlockBlasted++;
                    currentBlockBlasted++;
                    
                }
                else if (matchList[i].gameObject.CompareTag("pink") && pinkBlockBlasted <= blastNumberOfEachColorBlocks)
                {
                    pinkBlockBlasted++;
                    currentBlockBlasted++;
                    
                }
                else if (matchList[i].gameObject.CompareTag("green") && greenBlockBlasted <= blastNumberOfEachColorBlocks)
                {
                    greenBlockBlasted++;
                    currentBlockBlasted++;
                    
                }
                else if (matchList[i].gameObject.CompareTag("yellow") && yellowBlockBlasted <= blastNumberOfEachColorBlocks)
                {
                    yellowBlockBlasted++;
                    currentBlockBlasted++;
                    
                }
                else if (matchList[i].gameObject.CompareTag("purple") && purpleBlockBlasted <= blastNumberOfEachColorBlocks)
                {
                    purpleBlockBlasted++;
                    currentBlockBlasted++;
                    
                }

                if (i >= matchList.Count - 1) // When it comes to the last object to be blasted, a call is made to activate new color blocks.
                {
                    needNewColorBlocks = true;
                    
                }


            }

            

            if(maxMovementCount>0)   maxMovementCount--; //every time a blast is made, move 1 down
        }

        ColorBlocks.isTriyingBlast = false; // stop blast function

        
    }




    public void spawnNewColors(bool needColors) //Function defined to activate new objects after blast color blocks.
    {
        if (needColors)
        {

            //The loop's working logic: It starts with the lowest elements in the coordinate system of the array (starting with
            //colors[widht,height]) and checks if its elements are empty (if it should already be empty, that element is skipped).
            //If the element is empty, a new loop is started over the column it is in, and the ones at the top are shifted down
            //if they are not empty. If they are empty, array elements are closed with the objects taken from
            //the object pool so that they fall to the required coordinate from the top.

            

            for (int x = width - 1; x >= 0; x--)
            {
                

                for (int y = height - 1; y >= 0; y--)
                {

                    

                    if (boardLayout.rows[y].row[x]) // if array element should not be filled, skip that element
                    {
                        
                        continue;
                    }
                    else if (colors[x, y] != null && colors[x, y].activeSelf) // if array element not null continue to next array element
                    {
                        continue;
                    }
                    else if (colors[x, y] == null || colors[x, y].activeSelf == false)
                    {
                        


                        for (int i = y - 1; i >= -1; i--) // start loop over column of array element that needs to be filled
                        {
                            if (i < 0 || i > height) break;

                            if (colors[x, y] != null && colors[x, y].activeSelf == true) // if element is filled break the loop
                            {
                                break;
                            }

                            
                            if ((colors[x, y] == null || colors[x, y].activeSelf == false) && (colors[x, i] != null && colors[x, i].activeSelf == true))
                            {



                                //If the checked element is empty and the element in its column is not empty, that element should
                                //slide down and new array indexes will be assigned. Also, before assigning a new array index,
                                //it is checked whether the element in it is null or setActive(false). If setActive(false),
                                //the color block is added to the object pool.

                                

                                if (colors[x, y] != null) { objectPool.Add(colors[x, y].gameObject); }
                                colors[x, y] = null;
                                colors[x, y] = colors[x, i].gameObject;
                                colors[x, i] = null;



                                // For the down movement, values are assigned in a script inside each color block and the the movement codes
                                // are executed in that script

                                colors[x, y].gameObject.GetComponent<ColorBlocks>().ArrayIndexX = x;
                                colors[x, y].gameObject.GetComponent<ColorBlocks>().arrayIndexY = y;
                                colors[x, y].gameObject.GetComponent<ColorBlocks>().isNeedMove = true;
                                
                                
                                
                                
                                colors[x, y].gameObject.name = colors[x, y].gameObject.tag + "[" + y.ToString() + ", " + x.ToString() + "]";

                                break;
                                
                            }

                            else if (i == 0 && (colors[x, i] == null || colors[x, i].activeSelf == false))
                            {

                                //When you reach the end of the loop in Column, if the array element is empty or setActive(false),
                                //fill that element with the color block you got from the object pool. And if the element you are
                                //going to fill is setActive(false) add that element to the object pool


                                int randomIndex = Random.Range(0, objectPool.Count);                                
                                RectTransform rect = objectPool[randomIndex].GetComponent<RectTransform>();
                                rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * 0) + 64*2); // Activate the object above the parent object that is in for the falling effect.


                                colors[x, y] = objectPool[randomIndex].gameObject;
                                objectPool.RemoveAt(randomIndex);
                                colors[x, y].SetActive(true);
                                

                                                                
                                if(colors[x, i] != null) { objectPool.Add(colors[x, i].gameObject); }
                                colors[x, i] = null;

                                // For the movement down, values are assigned in a script inside each color block and the the movement codes
                                // are executed in that script
                                colors[x, y].gameObject.GetComponent<ColorBlocks>().ArrayIndexX = x;
                                colors[x, y].gameObject.GetComponent<ColorBlocks>().arrayIndexY = y;
                                colors[x, y].gameObject.GetComponent<ColorBlocks>().isNeedMove = true;
                                colors[x, y].gameObject.name = colors[x, y].gameObject.tag + "[" + y.ToString() + ", " + x.ToString() + "]";

                                


                            }


                        }

                        if (y == 0 && (colors[x, y] == null || colors[x, y].activeSelf == false))
                        {

                            //Sometimes the code written above skip filling the elements in the 0th column.
                            //To avoid this situation, similar code was written above.


                            if (colors[x, y] != null) { objectPool.Add(colors[x, y].gameObject); }

                            int randomIndex = Random.Range(0, objectPool.Count);
                            RectTransform rect = objectPool[randomIndex].GetComponent<RectTransform>();
                            rect.anchoredPosition = new Vector2(32 + (64 * x), -32 - (64 * 0) + 64*3.5f);

                            colors[x, y] = objectPool[randomIndex].gameObject;
                            colors[x, y].SetActive(true);
                            objectPool.RemoveAt(randomIndex);    
                            

                            // For the movement down, values are assigned in a script inside each color block and the the movement codes
                            // are executed in that script
                            colors[x, y].gameObject.GetComponent<ColorBlocks>().ArrayIndexX = x;
                            colors[x, y].gameObject.GetComponent<ColorBlocks>().arrayIndexY = y;
                            colors[x, y].gameObject.GetComponent<ColorBlocks>().isNeedMove = true;
                            colors[x, y].gameObject.name = colors[x, y].gameObject.tag + "[" + y.ToString() + ", " + x.ToString() + "]";
                        }
                    }



                }
            }
            needNewColorBlocks = false; // stop assigning new color blocks

            if (checkMatches == false) // check if there is a new matches
            {
                checkMatches = true;
            }
        }


        
        
    }

    private void shuffleColorCubes() //shuffle color blocks
    {

        if (needShuffle)
        {
            needShuffle = false;                       
            needColors = true;                        
            shuffleText.gameObject.SetActive(false);
                                   
        }        
                
    }

    private void SuccessAndGameOver() // success and failure function
    {
        
        if(maxMovementCount == 0 && totalBlockShouldBlasted > currentBlockBlasted)
        {
            SuccessAndGameOverText.text = "Sorry! Game Over";
            SuccessAndGameOverText.gameObject.SetActive(true);
            isGameOver = true;
        }

        if(currentBlockBlasted >= totalBlockShouldBlasted && maxMovementCount>0)
        {
            SuccessAndGameOverText.text = "Well Done! You Completed The Task";
            SuccessAndGameOverText.gameObject.SetActive(true);
            isGameOver = true;
        }
    }

   
}
