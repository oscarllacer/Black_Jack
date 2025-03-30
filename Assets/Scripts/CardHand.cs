using System.Collections.Generic;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    public GameObject card;
    public bool isDealer = false;
    public int points;
    private int coordY;

    private void Awake()
    {
        points = 0;
        //Definimos dónde posicionamos las cartas de cada uno
        if (!isDealer)
            coordY = 2;
        else
            coordY = -2;
    }
   

    public void Clear()
    {
        points = 0;
        if (!isDealer)
            coordY = 2;
        else
            coordY = -2;
        foreach (GameObject g in cards)
        {
            Destroy(g);
        }
        cards.Clear();
    }

    public void InitialToggle()
    {
        cards[0].GetComponent<CardModel>().ToggleFace(true);
    }

    public void Push(Sprite front, int value)
    {
        

        //Creamos una carta y la añadimos a nuestra mano
        GameObject cardCopy = (GameObject)Instantiate(card);
        cards.Add(cardCopy);

        //La posicionamos en el tablero 
        float coordX = (float)1.4 * (float)(cards.Count - 3);
        Vector3 pos = new Vector3(coordX, coordY);
        cardCopy.transform.position = pos;

        //Le ponemos la imagen y el valor asignado
        cardCopy.GetComponent<CardModel>().front = front;
        cardCopy.GetComponent<CardModel>().value = value;

        //La cubrimos si es la primera del dealer
        if (isDealer && cards.Count <= 1)
            cardCopy.GetComponent<CardModel>().ToggleFace(false);
        else
            cardCopy.GetComponent<CardModel>().ToggleFace(true);
        // Actualizamos points con el nuevo valor
        UpdatePoints();
       
    }



    private void UpdatePoints()
    {
        int total = 0;
        int numAces = 0;
        // Sumar valores de cartas, contando ases por separado
        foreach (var card in cards)
        {
            CardModel cardModel = card.GetComponent<CardModel>();

            if (cardModel != null)
            {
                if (cardModel.value == 11)
                    numAces++;
                else
                    total += cardModel.value;
            }
        }

        // Ajuste de valores de Ases para evitar pasarse de 21
        while (numAces > 0)
        {
            if (total + 11 <= 21)
                total += 11;
            else
                total += 1;

            numAces--;
        }

        points = total;
    }



}