using System;
using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{

    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text prob1;
    public Text prob2;
    public Text prob3;
    public Text pointsPlayer;
    public Text pointsDealer;
    public Dropdown betDropdown;
    public Text bankText;
    public Text finalApuestaMessage;
    public Button confirmButton;
    public int[] values = new int[52];
    int cardIndex = 0;
    int[] order = new int[52];
    private int bank = 1000;
    private int bet = 0;
    private void Awake()
    {
        InitCardValues();
    }
    private void Start()
    {
        ShuffleCards();
        StartGame();
        confirmButton.onClick.AddListener(OnConfirm);
        bank = 1000;
        betDropdown.onValueChanged.AddListener(OnBetDropdownChanged);
        UpdateBankText();
    }
    
    

    private void InitCardValues()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (i % 13) + 1;
            if (values[i] > 10)
            {
                values[i] = 10;
            }
            if (values[i] == 1)
            {
                // Si el valor actual es un As (1), asignamos 11 a menos que 
                // al hacerlo se exceda el límite de 21, en cuyo caso, asignamos 1.
                if (dealer.GetComponent<CardHand>().points + 11 <= 21 || player.GetComponent<CardHand>().points + 11 <= 21)
                {
                    values[i] = 11;
                }
                else
                {
                    values[i] = 1;
                }
            }
        }
    }


    private void ShuffleCards()
    {
        System.Random rng = new System.Random();
        int n = values.Length;

        // Inicializar el orden con valores del 0 al 51
        for (int i = 0; i < n; i++)
        {
            order[i] = i;
        }

        // Barajar el orden de las cartas
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);

            // Intercambiar en el array order[]
            int tempOrder = order[k];
            order[k] = order[n];
            order[n] = tempOrder;
        }
    }


    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        // Verificar Blackjacks iniciales 
        if (GetPlayerScore() == 21)
        {
            finalMessage.text = "El jugador tiene Blackjack!";
        }
        else if (GetDealerScore() == 21)
        {
            finalMessage.text = "El crupier tiene Blackjack!";
        }
        else // Si no hay Blackjack, se habilita la opción de pedir carta
        {
            hitButton.interactable = true;
            stickButton.interactable = true;
        }
    }


    private void CalculateProbabilities()
    {
        float probability1 = 0;
        float probability2 = 0;
        float probability3 = 0;

        int remainingCards = values.Length - cardIndex;

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas,
        //probabilidad de que teniendo la carta oculta, el dealer tenga más puntuación que el jugador
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (dealer.GetComponent<CardHand>().points + values[i] > player.GetComponent<CardHand>().points)
            {
                probability1++;
            }
        }

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas,
        //probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (player.GetComponent<CardHand>().points + values[i] >= 17 && player.GetComponent<CardHand>().points + values[i] <= 21)
            {
                probability2++;
            }
        }

        //calcular la probabilidad de que sabiendo que la baraja del blackjack tiene 52 cartas,
        //probabilidad de que el jugador obtenga más de 21 si pide una carta
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (player.GetComponent<CardHand>().points + values[i] > 21)
            {
                probability3++;
            }
        }

        if (remainingCards != 0)
        {
            prob1.text = Math.Round((probability1 / remainingCards), 4).ToString();
            prob2.text = Math.Round((probability2 / remainingCards), 4).ToString();
            prob3.text = Math.Round((probability3 / remainingCards), 4).ToString();
        }
        else
        {
            prob1.text = "0";
            prob2.text = "0";
            prob3.text = "0";
        }
    }


    void PushDealer()
    {
        // Asegurarse de que no se repartan más cartas de las que hay en la baraja
        if (cardIndex >= faces.Length)
        {
            Debug.Log("No hay más cartas en la baraja.");
            return;
        }

        int cardPos = order[cardIndex]; // Usar el índice barajado
        // Repartir la carta al crupier
        dealer.GetComponent<CardHand>().Push(faces[order[cardIndex]], values[order[cardIndex]]);
        cardIndex++;

        CalculateProbabilities();
    }


    void PushPlayer()
    {
        // Asegurarse de que no se repartan más cartas de las que hay en la baraja
       if (cardIndex >= faces.Length)
    {
        Debug.Log("No hay más cartas en la baraja.");
        return;
    }

    int cardPos = order[cardIndex]; // Usar el índice barajado
    player.GetComponent<CardHand>().Push(faces[cardPos], values[cardPos]);
    cardIndex++;

    pointsPlayer.text = player.GetComponent<CardHand>().points.ToString();

    // Si el jugador alcanza 21, mostrar "Blackjack!!"
    if (GetPlayerScore() == 21)
    {
        finalMessage.text = "¡Blackjack!!";
        Stand(); // Detener al jugador (no puede seguir pidiendo cartas)
    }
    else if (GetPlayerScore() > 21)
    {
            finalMessage.text = "¡¡¡Has perdido, te has pasado de 21!!!";
            Stand(); // Si el jugador se pasa de 21, también se detiene
        }

    CalculateProbabilities();
    }


    public void Hit()
    {
        PushPlayer();

        // Desactiva el Dropdown y el botón de confirmación para que el jugador no pueda cambiar su apuesta
        betDropdown.interactable = false;
        confirmButton.interactable = false;
    }


    public void Stand()
    {
        hitButton.interactable = false;
        int playerScore = GetPlayerScore(); // Obtener el puntaje del jugador

        // Si el jugador se pasó de 21, no dejar que el dealer juegue
        if (playerScore > 21)
        {
            ShowResult(-1); // -1 indica que el jugador perdió
            return; // Salir de la función para evitar que el dealer tome cartas
        }


        // Obtener la mano del dealer
        CardHand dealerHand = dealer.GetComponent<CardHand>();

        // Mostrar la cara de la carta del dealer que está boca abajo

        dealerHand.cards[0].GetComponent<CardModel>().ToggleFace(true);
        

        // Lógica del dealer (pedir hasta llegar a 17 o más)
        while (GetDealerScore() < 17)
        {
            PushDealer();
        }

        int winner = DetermineWinner();
        ShowResult(winner);

        CalculateProbabilities();
    }


    private int DetermineWinner()
    {
        int playerScore = GetPlayerScore();
        int dealerScore = GetDealerScore();

        // Lógica considerando pasarse de 21
        if (playerScore > 21)
        {
            OnLose();
            return 2; // Dealer gana
        }
        else if (dealerScore > 21)
        {
            OnWin();
            return 1; // Jugador gana
        }
        // Resto de la lógica de comparación...
        else if (playerScore == dealerScore)
        {
            //  Si hay empate, devolver la apuesta
            finalMessage.text = "Empate. Tu apuesta te ha sido devuelta.";
            bank += bet;  // Devolver la apuesta
            UpdateBankText();
            return 0; // Empate
        }
        else if (playerScore > dealerScore)
        {
            OnWin();
            return 1; // Jugador gana
        }
        else
        {
            OnLose();
            return 2; // Dealer gana
        }
    }


    private void ShowResult(int winner)
    {
        switch (winner)
        {
            case 0: // Empate
                finalMessage.text = "Empate";
                break;
            case 1: // Jugador gana
                finalMessage.text = "¡Has ganado!";
                OnWin();
                break;
            case 2: // Dealer gana
                finalMessage.text = "El dealer ha ganado";
                break;
        }
        stickButton.interactable = false;
    }


    public void PlayAgain()
    {

        if (bank <= 0)
        {
            bank = 1000; // Reiniciar banca si el jugador está en bancarrota
        }
        hitButton.interactable = true;
        stickButton.interactable = true;
        betDropdown.interactable = true;
        confirmButton.interactable = true;
        playAgainButton.interactable = true; // Asegurar que el botón sigue activo
        
        //Limpiar todos los mensajes de texto  

        finalMessage.text = "";
        finalApuestaMessage.text = "";
        prob1.text = "";
        prob2.text = "";
        prob3.text = "";

        // Reinicia la partida limpiando las manos, barajando y comenzando el juego.
        
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
        UpdateBankText();
    }


    public int GetPlayerScore()
    {
        // Actualiza y devuelve la puntuación del jugador
        pointsPlayer.text = player.GetComponent<CardHand>().points.ToString();
        return player.GetComponent<CardHand>().points;
    }


    public int GetDealerScore()
    { 
        // Actualiza y devuelve la puntuación del dealer
        pointsDealer.text = dealer.GetComponent<CardHand>().points.ToString();
        return dealer.GetComponent<CardHand>().points;
    }


    public int GetRemainingCards()
    {
        // Devuelve la cantidad de cartas restantes en el mazo
        return values.Length - cardIndex;
    }


    public void OnConfirm()
    {

        // Procesa la apuesta seleccionada por el jugador.
        string betText = betDropdown.options[betDropdown.value].text.Replace(" Credits", "");
        int betValue;
        if (int.TryParse(betText, out betValue))
        {
            if (betValue > bank)
            {
                finalApuestaMessage.text = "No tienes suficiente dinero para hacer esa apuesta";
            }
            else
            {
                //Resta la apuesta del banco y la guarda.
                bet = betValue;
                bank -= bet;
                UpdateBankText();
            }
        }
        else
        {
            Debug.LogError("Valor de apuesta no válido: " + betDropdown.options[betDropdown.value].text);
        }
    }


    public void OnWin()
    {
        // Aumenta el banco con la ganancia de la apuesta.
        bank += bet * 2 - bet;
        UpdateBankText();
    }


    public void OnLose()
    {
        // Actualiza el banco y verifica si el jugador se quedó sin dinero
        UpdateBankText();
        if (bank <= 0)
        {
            EndGame();
        }
    }


    private void UpdateBankText()
    {
        bankText.text = bank.ToString() + "€";
    }


    private void EndGame()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        //playAgainButton.interactable = false;
        betDropdown.interactable = false;
        confirmButton.interactable = false;
        finalApuestaMessage.text = "¡Has perdido! No te queda más dinero";
    }


    public void OnBetDropdownChanged(int index)
    {
    finalApuestaMessage.text = "";
    }
}