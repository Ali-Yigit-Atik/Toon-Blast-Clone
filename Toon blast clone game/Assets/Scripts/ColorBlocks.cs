using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorBlocks : MonoBehaviour, IPointerDownHandler
{
    public static bool isTriyingBlast = false; // It is used to give information to MatchAndBlast script whether it has tried blast or not.
    public Sprite[] colorSprites; // are used for changing icons



    public static string gameObjectName; // using for MatchAndBlast script to check game object
    
    private RectTransform rect; // is use for movement
    private Vector2 targetRect; // is use for movement   
    public bool isNeedMove = false; // condition of movement
    float speed = 0f;

    public int ArrayIndexX; // x index of color block in colors array
    public int arrayIndexY; // y index of color block in colors array



    private void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
        
    }

    private void Update()
    {
        


        if (isNeedMove == true)
        {
            // Movement codes:
            // In order to avoid possible bugs, it confirms that the element in the colors array is not null and is still
            // this color block. If SetActive(false) is not needed in the array, no action is taken.

            //After the confirmations are made, their speed is adjusted according to the column index in terms of visuality.
            //The color block is moved to the location where it should be with the MoveTowards method.

            if ((MatchAndBlast.colors[ArrayIndexX, arrayIndexY] != null && MatchAndBlast.colors[ArrayIndexX, arrayIndexY].activeSelf == true))
            {
                if (gameObject.name == MatchAndBlast.colors[ArrayIndexX, arrayIndexY].gameObject.name)
                {


                    targetRect = new Vector2(32 + (64 * ArrayIndexX), -32 - (64 * arrayIndexY));

                    
                    
                    if (arrayIndexY == 0 || arrayIndexY == 1)
                    {
                        speed = 450f;
                    }
                    if (arrayIndexY == 2)
                    {
                        speed = 600f;
                    }
                    if (arrayIndexY >= 3)
                    {
                        speed = 700f;
                    }
                    


                    rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, targetRect, Time.deltaTime * speed);
                    if (rect.anchoredPosition == targetRect) isNeedMove = false;

                }

            }
        }

    }


    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) // take information if player is triying blast color blocks
    {
        isTriyingBlast = true;        
        gameObjectName = gameObject.name;

    }
}
