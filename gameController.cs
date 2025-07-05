using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gameController : MonoBehaviour
{
    [SerializeField] private Color cardDefaultColor = new Color(196f, 255f, 252f); // Default color for cards
    private List<CardData> cardsFlipped = new List<CardData>(); // Create a list of cards that are flipped
    private int matchesFound = 0, attemptsLeft = 5, wins = 0;
    public GameObject[] cardObjects; // Array to hold card GameObjects
    public GameObject playAgainButton, youWinText, titleText, attemptsLeftText, winsText, background; // UI elements for the game
    public AudioClip winSound, loseSound; // Sound to play when the player wins
    public audio audioManager; // Audio manager to play sounds

    // A struct to hold the data for each card
    private struct CardData
    {
        public GameObject gameObject;
        public int value;
        public Color color;
        public Vector2 position;

        public CardData(GameObject obj, int val, Color col, Vector2 pos)
        {
            gameObject = obj;
            value = val;
            color = col;
            position = pos;
        }
    }

    // 2D array to store CardData
    private CardData[,] cards;

    private void Start()
    {
        int cardCount = 0;
        List<Color> cardColors = new List<Color> { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.red, Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta };

        // Shuffle the card colors using Fisher-Yates shuffle algorithm
        int n = cardColors.Count;
        while (n > 1)
        {
            n--;
            int R = Random.Range(0, n + 1);
            Color Col = cardColors[R];
            cardColors[R] = cardColors[n];
            cardColors[n] = Col;
        }

        // Initialize the 2D array
        int rows = 6;
        int cols = 2;
        cards = new CardData[rows, cols];

        // Give each card in the array a GameObject, value, color, and flipped state
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                GameObject obj = cardObjects[cardCount];
                int val = cardCount;
                Color col = cardColors[cardCount];
                Vector2 pos = new Vector2(0, 0);

                if (cardCount <= 5)
                {
                    float spacing = 33000f; // Spacing between cards
                    float startX = -81500;

                    float xPosition = startX + (cardCount * spacing);
                    float yPosition = 26000f;
                    pos = Camera.main.ScreenToWorldPoint(new Vector3(xPosition, yPosition, 0));

                }
                else
                {
                    float spacing = 33000f; // Spacing between cards
                    float startX = -81500;

                    float xPosition = startX + ((cardCount - 6) * spacing);
                    float yPosition = -26000f;
                    pos = Camera.main.ScreenToWorldPoint(new Vector3(xPosition, yPosition, 0));
                }

                cards[i, j] = new CardData(obj, val, col, pos);
                cardCount++;
            }
        }

        SetCardPositionsAndColor(); // Set the initial positions of the cards
    }

    public void CardClicked(int val)
    {
        // Loop through the 2D array to find the card with the specified value
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                if (cards[i, j].value == val) // If the card value given is equal to the value of the card in the array
                {
                    cards[i, j].gameObject.GetComponent<Image>().color = cards[i, j].color;  // Change the cards color
                    cards[i, j].gameObject.GetComponent<Button>().interactable = false;                     // Make the card unclickable
                    cards[i, j].gameObject.GetComponent<Animator>().Play("Flip");                           // Play the flip animation
                    cardsFlipped.Add(cards[i, j]);                                                          // Add the card to the flipped list
                    CheckIfTwoCardsFlipped();                                                               // Check if two cards are flipped  
                    return;
                }
            }
        }
    }

    private void FlipAllCards()
    {
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                cards[i, j].gameObject.GetComponent<Image>().color = cards[i, j].color;  // Change the cards color
                cards[i, j].gameObject.GetComponent<Button>().interactable = false;                     // Make the card unclickable
                cards[i, j].gameObject.GetComponent<Animator>().Play("Flip");                           // Play the flip animation   
            }
        }
    }

    private void CheckIfTwoCardsFlipped()
    {
        if (cardsFlipped.Count == 2)
        {
            MakeCardsInteractable(false); // Make all cards unclickable while checking for a match

            if (cardsFlipped[0].color == cardsFlipped[1].color)
            {
                matchesFound++; // Increment the matches found counter
                Invoke("CheckWinCondition", 0.5f);
                StartCoroutine(LerpCardsToBottom()); // Start the coroutine to move cards down
            }
            else
            {
                Invoke("FlipBackCards", 1f);

                attemptsLeft--; // Decrease the attempts left counter
                attemptsLeftText.GetComponent<TMP_Text>().text = "Attempts Left: " + attemptsLeft;
                //attemptsLeftText.GetComponent<Animation>().Play("Shake"); // Play the shake animation on attempts left text

                if (attemptsLeft <= 0) // If no attempts left
                {
                    Invoke("YouLose", 1f);
                }
                else if
                (attemptsLeft == 1) // If attempts left
                {
                    attemptsLeftText.GetComponent<TMP_Text>().color = Color.red;
                }
            }
        }
    }

    public void FlipBackCards()
    {
        for (int i = 0; i < cardsFlipped.Count; i++)
        {
            cardsFlipped[i].gameObject.GetComponent<Image>().color = cardDefaultColor; // Reset the color to light blue
            cardsFlipped[i].gameObject.GetComponent<Button>().interactable = true; // Make the card clickable again
            cardsFlipped[i].gameObject.GetComponent<Animator>().Play("Flip"); // Play the flip animation to flip back
        }
        cardsFlipped.Clear(); // Clear the flipped cards list
        MakeCardsInteractable(true); // Make all cards clickable again
    }

    private void MakeCardsInteractable(bool interactable)
    {
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                cards[i, j].gameObject.GetComponent<Button>().interactable = interactable;
            }
        }
    }

    // Lerps paired cards to the bottom of the screen
    private System.Collections.IEnumerator LerpCardsToBottom()
    {
        float elapsedTime = 0f;
        float waitTime = 0.5f;

        while (elapsedTime < waitTime)
        {
            cardsFlipped[0].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cardsFlipped[0].gameObject.GetComponent<Transform>().position, new Vector2(900, -350), (elapsedTime / (waitTime + 10f))); // Move the card down
            cardsFlipped[1].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cardsFlipped[1].gameObject.GetComponent<Transform>().position, new Vector2(900, -350), (elapsedTime / (waitTime + 10f))); // Move the card down
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        cardsFlipped[0].gameObject.GetComponent<Transform>().position = new Vector2(900, -350); // Move the card down
        cardsFlipped[1].gameObject.GetComponent<Transform>().position = new Vector2(900, -350); // Move the card down

        cardsFlipped.Clear();
        MakeCardsInteractable(true);
    }

    private System.Collections.IEnumerator LerpAllCardsToBottom()
    {
        float elapsedTime = 0f;
        float waitTime = 0.75f;

        while (elapsedTime < waitTime)
        {
            for (int i = 0; i < cards.GetLength(0); i++)
            {
                for (int j = 0; j < cards.GetLength(1); j++)
                {
                    cards[i, j].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cards[i, j].gameObject.GetComponent<Transform>().position, new Vector2(900, -350), (elapsedTime / (waitTime + 20f))); // Move the card down
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // Check if all matches are found
    private void CheckWinCondition()
    {
        if (matchesFound == 6) // If all matches are found
        {
            youWinText.GetComponent<TMP_Text>().text = "You Win!";
            youWinText.SetActive(true);
            audioManager.PlaySound(winSound, .5f); // Play win sound
            wins++;
            winsText.GetComponent<TMP_Text>().text = "Wins: " + wins; // Update the wins text
            Invoke("GoBackToMenu", 3f);
        }
    }

    private void GoBackToMenu()
    {
        HideCards(true);
        playAgainButton.SetActive(true);
        titleText.GetComponent<TMP_Text>().enabled = true; // Enable the title text
        youWinText.SetActive(false);
        attemptsLeftText.SetActive(false);
        winsText.SetActive(true);
        background.SetActive(true); // Show the background
        Reset(); // Reset the game state
    }

    public void HideCards(bool hide)
    {
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                if (hide)
                    cards[i, j].gameObject.SetActive(false);
                else
                    cards[i, j].gameObject.SetActive(true);
            }
        }
    }

    private void YouLose()
    {
        FlipAllCards(); // Flip all cards back to their default state
        Invoke("callLerp", 0.5f); // Lerp all cards to the bottom
        youWinText.GetComponent<TMP_Text>().text = "You Lose!";
        youWinText.SetActive(true);
        audioManager.PlaySound(loseSound, .1f); // Play lose sound
        Invoke("GoBackToMenu", 2.5f);
    }

    private void callLerp()
    {
        StartCoroutine(LerpAllCardsToBottom());
    }

    private void Reset()
    {
        attemptsLeft = 5; // Reset attempts left
        attemptsLeftText.GetComponent<TMP_Text>().text = "Attempts Left: " + attemptsLeft;
        attemptsLeftText.GetComponent<TMP_Text>().color = Color.black; // Reset attempts left text color
        matchesFound = 0; // Reset matches found
        SetCardPositionsAndColor(); // Reset card positions
        Start(); // Reinitialize the game
    }

    private void SetCardPositionsAndColor()
    {
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                cards[i, j].gameObject.GetComponent<RectTransform>().position = cards[i, j].position; // Set the position of the card
                cards[i, j].gameObject.GetComponent<Image>().color = cardDefaultColor; // Set the color of the card
            }
        }
    }

    public void OnCardEnable()
    {
        // Use button
        // Lerp cards to their original positions

        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                cards[i, j].gameObject.transform.position = new Vector2(-150, -300); // Set the position of the card
            }
        }

        StartCoroutine(LerpCardsToPositions()); // Start the coroutine to lerp cards to their positions
    }

    private System.Collections.IEnumerator LerpCardsToPositions()
    {
        float elapsedTime = 0f;
        float waitTime = .25f;

        // Lerp the cards all at once
        /*
        while (elapsedTime < waitTime)
        {
            for (int i = 0; i < cards.GetLength(0); i++)
            {
                for (int j = 0; j < cards.GetLength(1); j++)
                {
                    cards[i, j].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cards[i, j].gameObject.GetComponent<Transform>().position, cards[i, j].position, (elapsedTime / (waitTime + 25f))); // Move the card down
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }*/

        // Lerp the cards one by one
        for (int i = 0; i < cards.GetLength(0); i++)
        {
            for (int j = 0; j < cards.GetLength(1); j++)
            {
                while (elapsedTime < waitTime)
                {
                    cards[i, j].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cards[i, j].gameObject.GetComponent<Transform>().position, cards[i, j].position, (elapsedTime / (waitTime + 0f))); // Move the card down
                    elapsedTime += Time.deltaTime;
                    yield return null; // Wait for the next frame
                }
                elapsedTime = 0f; // Reset elapsed time for the next card
            }
        }
    }

    /*
    --NOT WORKING--
    Recursive function to move cards to their positions

    private System.Collections.IEnumerator recursive(int calls, int i)
    {
        
        float elapsedTime = 0f;
        float waitTime = .5f;
        int j = 0;

        if (calls > 13)
        {
            yield break; // Stop recursion after 10 calls
        }

        Debug.Log("Recursive call: " + calls + ", i: " + i + " j: " + j);
        StartCoroutine(recursive(calls + 1, i + 1)); // Recursive call to the next card
        Debug.Log("Row: " + i + ", Col: " + j);
        while (elapsedTime < waitTime)
        {
            if (i > 6)
            {
                j = 1;
                i = i - 6;
            }

            cards[i, j].gameObject.GetComponent<Transform>().position = Vector2.Lerp(cards[i, j].gameObject.GetComponent<Transform>().position, cards[i, j].position, (elapsedTime / waitTime + 15)); // Move the card to its position

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    }
    */

    public void PlaySound(AudioClip clip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1f; // Set default volume
        audioSource.PlayOneShot(clip);
    }
}
