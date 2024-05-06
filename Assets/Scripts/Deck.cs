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
    public Text playerText;
    public Text dealerText;

    public int[] values = new int[52];
    int cardIndex = 0;    

    private void Awake(){    
        InitCardValues();        
    }

    private void Start(){
        ShuffleCards();
        StartGame();        
    }

    private void InitCardValues(){
        for (int i = 0; i < 52; i++){
            // Obtenemos el valor de la carta basado en su posición
            int cardValue = (i % 13) + 1; // Los valores van de 1 a 13 con el residuo de i/13 (As a Rey)

            // Si el valor de la carta excede 10, establecemos su valor en 10 (J, Q, K)
            if (cardValue > 10){
                cardValue = 10;
            }

            // Asignamos el valor de la carta al array values
            values[i] = cardValue;
        }
    }
    private void ShuffleCards(){ //--------------------------------------
            // Creamos un nuevo array para almacenar temporalmente las cartas mezcladas 
            Sprite[] shuffledFaces = new Sprite[faces.Length];
            int[] shuffledValues = new int[values.Length];

            // Copiamos las cartas originales al array temporal
            faces.CopyTo(shuffledFaces, 0);
            values.CopyTo(shuffledValues, 0);

            // Usamos el algoritmo de Fisher-Yates para mezclar las cartas
            for (int i = 0; i < shuffledFaces.Length - 1; i++){
                int randomIndex = Random.Range(i, shuffledFaces.Length);
                // Intercambiamos la carta en la posición i con la carta en la posición aleatoria
                Sprite tempFace = shuffledFaces[i];
                shuffledFaces[i] = shuffledFaces[randomIndex];
                shuffledFaces[randomIndex] = tempFace;

                int tempValue = shuffledValues[i];
                shuffledValues[i] = shuffledValues[randomIndex];
                shuffledValues[randomIndex] = tempValue;
            }

            // Asignamos las cartas mezcladas de vuelta a los arreglos originales
            shuffledFaces.CopyTo(faces, 0);
            shuffledValues.CopyTo(values, 0);
        }
    private void StartGame(){
        dealerText.gameObject.SetActive(false);
        for (int i = 0; i < 2; i++){
            PushPlayer();
            PushDealer();
        }

        // Verificar si alguno de los jugadores tiene Blackjack
        if (player.GetComponent<CardHand>().points == 21){
            // Mensaje de jugador con Blackjack
                finalMessage.text = "¡Blackjack! Has ganado.";
        }
        else if (dealer.GetComponent<CardHand>().points == 21){
            // Mensaje de jugador con Blackjack
            finalMessage.text = "¡Derrota! El dealer ha ganado.";
        }
    }

    private float ProbabilityDealerWins()
    {
        int playerScore = player.GetComponent<CardHand>().points;
        int dealerScore = dealer.GetComponent<CardHand>().points;
        int remainingPoints = 21 - dealerScore; // Puntos restantes para que el crupier alcance 21

        // Contar las cartas que no hagan que la puntuación del crupier supere la del jugador
        int cardsDealerCanDraw = 0;
        int totalCards = 0;

        foreach (int value in values)
        {
            if (dealerScore + value <= playerScore && dealerScore + value <= 21)
            {
                cardsDealerCanDraw++;
            }
            totalCards++;
        }

        // Calcular la probabilidad como el número de cartas que no hacen que el crupier supere la puntuación del jugador dividido por el total de cartas
        float probability = (float)cardsDealerCanDraw / totalCards;

        return probability;
    }


    private float Probability17to21()
    {
        int playerScore = player.GetComponent<CardHand>().points;
        int remainingPoints = 21 - playerScore; // Puntos restantes para alcanzar 21

        // Contar las cartas que hagan que la puntuación del jugador esté entre 17 y 21
        int cardsBetween17and21 = 0;
        int totalCards = 0;

        foreach (int value in values)
        {
            if (value >= 17 && value <= remainingPoints)
            {
                cardsBetween17and21++;
            }
            totalCards++;
        }

        // Calcular la probabilidad como el número de cartas que hagan que la puntuación del jugador esté entre 17 y 21 dividido por el total de cartas
        float probability = (float)cardsBetween17and21 / totalCards;

        return probability;
    }


    private float ProbabilityOver21()
    {
        int playerScore = player.GetComponent<CardHand>().points;

        // Contar las cartas que harían que la puntuación del jugador supere 21
        int cardsOver21 = 0;
        int totalCards = 0;

        foreach (int value in values)
        {
            if (playerScore + value > 21)
            {
                cardsOver21++;
            }
            totalCards++;
        }

        // Calcular la probabilidad como el número de cartas que hagan que la puntuación del jugador supere 21 dividido por el total de cartas
        float probability = (float)cardsOver21 / totalCards;

        return probability;
    }


    private void CalculateProbabilities()
    { //--------------------------------------

        //* Calcular las probabilidades de:
        //* - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
        float prob1 = ProbabilityDealerWins();
        //* - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
        float prob2 = Probability17to21();
        //* - Probabilidad de que el jugador obtenga más de 21 si pide una carta     
        float prob3 = ProbabilityOver21();


        // Mostrar los resultados en pantalla
        probMessage.text =
        "Dealer Wins:" + prob1.ToString("P2") + "\n" +
        "X <= 21: " + prob2.ToString("P2") + "\n" +
        "17<= X <= 21: " + prob3.ToString("P2");
    }

    void PushDealer(){
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;
    }
    void writeDealer(){
        dealerText.gameObject.SetActive(true);
        int dealerScore = dealer.GetComponent<CardHand>().points;
        dealerText.text = dealerScore.ToString();
    }

    void PushPlayer(){
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
        CalculateProbabilities();
        int playerScore = player.GetComponent<CardHand>().points;
        playerText.text = playerScore.ToString();
    }

    public void Hit(){ //--------------------------------------
        //Repartimos carta al jugador
        PushPlayer();
        int playerScore = player.GetComponent<CardHand>().points;

        if (playerScore > 21){
            dealer.GetComponent<CardHand>().InitialToggle(true);
            writeDealer();
            finalMessage.text = "¡Has perdido! Tu puntuación es mayor a 21.";
        }
        else if (playerScore == 21){
            dealer.GetComponent<CardHand>().InitialToggle(true);
            writeDealer();
            finalMessage.text = "¡Has ganado! Tu puntuación es 21.";
        }
    }

    public void Stand(){  //--------------------------------------
        /*TODO: 
        * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
        */

        dealer.GetComponent<CardHand>().InitialToggle(true);

        /*TODO:
        * Repartimos cartas al dealer si tiene 16 puntos o menos
        * El dealer se planta al obtener 17 puntos o más
        * Mostramos el mensaje del que ha ganado*/

        int dealerScore = dealer.GetComponent<CardHand>().points;
        while (dealerScore <= 16){
            PushDealer(); // Repartir una carta al crupier
            dealerScore = dealer.GetComponent<CardHand>().points;
            writeDealer();
        }

        // Mostrar mensaje del que ha ganado
        int playerScore = player.GetComponent<CardHand>().points;
        if (dealerScore > 21 || (playerScore <= 21 && playerScore > dealerScore)){
            // El jugador gana
            finalMessage.text = "¡Has ganado!";
        }
        else if (playerScore == dealerScore){
            // Empate
            finalMessage.text = "¡Empate!";
        }
        else{
            // El crupier gana
            finalMessage.text = "¡El crupier ha ganado!";
        }
    }

    public void PlayAgain(){
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
}
