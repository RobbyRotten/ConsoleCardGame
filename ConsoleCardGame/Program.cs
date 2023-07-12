using System;

namespace ConsoleCardGame 
{    
    internal class Program
    {
        static void Main(string[] args)        {
                   
            var playingTable = new Table();
            // playingTable.ShowTotalCards();
            int PlayersNum = 2;
            for (int i = 0; i < PlayersNum; i++)
            {
                bool isUser = false;
                string name = $"Player #{i}";
                if (i == 0)
                {
                    isUser = true;
                    name = "Me";
                }
                var player = new Player(isUser, name, i);
                player.Hand = playingTable.GiveCard(6);
                // player.ShowHand();
                playingTable.Players.Add(player);
            }
            playingTable.ShowTrumpSuit();
            playingTable.SetPlayersPriority();
            //playingTable.AddPlayersToRound();
            int storedRound = 0;
            while (!playingTable.GameEnd)
            {
                if (storedRound != playingTable.Round)
                {
                    foreach (Player player in playingTable.Players)
                    {
                        int cardsToTake = 6 - player.Hand.Count;
                        if (cardsToTake > 0)
                        {
                            player.Hand.AddRange(playingTable.GiveCard(cardsToTake));
                        }
                    }
                    //playingTable.AddPlayersToRound();
                    playingTable.ShowRound();
                    storedRound = playingTable.Round;
                    playingTable.CardsToBeat.Clear();
                }                
                foreach (Player player in playingTable.Players)
                {
                    player.MakeMove(playingTable);
                }
            }
        }
    }

    public class CardPair
    {
        public int Id;
        public Card? Card1st;
        public Card? Card2nd;
        public bool Beaten = false;

        public CardPair(Card card1st)
        {
            Card1st = card1st;
        }

        public void BeatCard(Card beatingCard)
        {
            Card2nd = beatingCard;
            Id = beatingCard.Id;
            Beaten = true;
        }
    }

    public class Table
    {
        public List<Player> Players = new List<Player>();
        public List<Card> TotalCards = new List<Card>();
        public List<CardPair> CardsToBeat = new List<CardPair>();
        public string TrumpSuit;
        public bool GameEnd = false;
        public int Round;
        public Dictionary<int, List<Player>> PlayersInRound = new Dictionary<int, List<Player>>();
        // public Card? LastplayedCard;

        public Table() 
        {
            Round = 1;
            var Suits = new List<string> { "♥", "♦", "♣", "♠" };
            Random random = new Random();
            TrumpSuit = Suits[random.Next(Suits.Count)];
            var CardValues = new List<string>();
            for (int i = 6; i < 11; i++)
            {
                CardValues.Add(i.ToString());
            }
            CardValues.AddRange(new List<string> { "J", "Q", "K", "A" });
            int id = 0;
            for (int i = 0; i < CardValues.Count; i++)
            {
                foreach (string suit in Suits)
                {
                    var card = new Card(id, suit, CardValues[i], i);
                    TotalCards.Add(card);
                    id++;
                }
            }
        }

        public void AddPlayersToRound()
        {
            PlayersInRound[Round] = Players;
        }

        public void ShowRound()
        {
            Console.WriteLine($"Round #{Round}. Trump Suit: {TrumpSuit}");
        }
        
        public List<Card> GiveCard(int cardsNum)
        {
            Random random = new Random();
            List<Card> cardsOut = new List<Card>();
            for (int i = 0; i < cardsNum; i++)
            {
                if (TotalCards.Count == 0)
                {
                    return cardsOut;
                }
                int index = random.Next(0, TotalCards.Count);
                cardsOut.Add(TotalCards[index]);
                TotalCards.RemoveAt(index);
            }
            return cardsOut;
        }

        public void ShowTotalCards()
        {
            List<string> listOut = new List<string>();
            foreach (Card card in TotalCards)
            {
                listOut.Add(card.ValueOf());
            }
            Console.WriteLine(string.Join(" ", listOut));
        }

        public void ShowTrumpSuit()
        {
            Console.WriteLine($"Trump Suit: {TrumpSuit}");
        }

        public Card? LastCardToBeat()
        {
            var LastPair = CardsToBeat.LastOrDefault();
            if (LastPair == null)
            {
                return null;
            }
            return LastPair.Card1st;
        }
        
        public int? LastCardToBeatIndex()
        {
            var LastPair = CardsToBeat.LastOrDefault();
            if (LastPair == null)
            {
                return null;
            }
            for (int i = 0; i < CardsToBeat.Count; i++)
            {
                if (CardsToBeat[i].Id == LastPair.Id)
                {
                    return i;
                }
            }
            return null;
        }

        public void BeatLastCard(Card card)
        {
            var ind = LastCardToBeatIndex();
            if (ind == null)
            {
                throw new Exception("Error: index is null");
            }
            CardsToBeat[ind.Value].Card2nd = card;
        }

        public void SetPlayersPriority()
        {   
            var maxTrumpDict = new Dictionary<int, int>();
            foreach (Player player in Players)
            {
                // Console.WriteLine($"{player.Name} {player.Id}");
                // player.ShowHand();
                foreach (Card card in player.Hand)
                {
                    if (card.Suit == TrumpSuit)
                    {
                        if (!maxTrumpDict.ContainsKey(player.Id) || maxTrumpDict[player.Id] < card.ValueInt)
                        {
                            maxTrumpDict[player.Id] = card.ValueInt;
                        }
                    }
                }
                if (!maxTrumpDict.ContainsKey(player.Id))
                {
                    maxTrumpDict[player.Id] = 0;
                }
            }
            //var sortedDict = from entry in maxTrumpDict orderby entry.Value descending select entry;
            var sortedDict = maxTrumpDict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var orderedPlayers = new List<Player>();
            foreach (KeyValuePair<int, int> entry in sortedDict)
            {
                var player = Players.Where(x => x.Id == entry.Key).FirstOrDefault();                
                if (player != null)
                {
                    var maxPlayersTrump = player.Hand.Where(x => x.ValueInt == entry.Value && x.Suit == TrumpSuit).FirstOrDefault();
                    if (maxPlayersTrump != null)
                    {
                        string message = $"You have: {maxPlayersTrump.ValueOf()}";
                        if (!player.IsUser)
                        {
                            message = $"Player {player.Name} has: {maxPlayersTrump.ValueOf()}";
                        }
                        Console.WriteLine(message);
                    }
                    else
                    {
                        string message = "You have no trump cards.";
                        if (!player.IsUser)
                        {
                            message = $"Player {player.Name} has no trump cards.";
                        }
                        Console.WriteLine(message);
                    }
                    orderedPlayers.Add(player);
                }
            }
            Players = orderedPlayers;
        }

    }

    public class Card
    {

        public int Id;
        public string Suit;
        public string Value;
        public int ValueInt;

        public Card(int id, string suit, string value, int valueInt)
        {
            Id = id; 
            Suit = suit; 
            Value = value;
            ValueInt = valueInt;
        }                

        public string ValueOf()
        {
            return $"{Suit}{Value}";
        }
    }

    public class Player
    {
        public int Id;
        public bool IsUser;
        public string Name;
        public List<Card> Hand;
        public bool IsDefending = false;        
        // public int Priority = 0;

        public Player(bool isUser, string name, int id)
        {
            IsUser = isUser;
            Hand = new List<Card>();
            Name = name;
            Id = id;    
        }

        public void ShowHand()
        {
            string cardsStr = "";
            string indexStr = "";
            for (int i=0; i<Hand.Count; i++)
            {
                cardsStr += Hand[i].ValueOf() + "\t";
                indexStr += i.ToString() + "\t";
            }
            Console.WriteLine(cardsStr);
            Console.WriteLine(indexStr);
        }

        public void MakeMove(Table playingTable)
        {
            if (IsUser)
            {
                MakeUserMove(playingTable);
            }
            else
            {                
                MakeComputerMove(playingTable);
            }
        }

        public void MakeUserMove(Table playingTable)
        {
            Console.WriteLine("Your hand: ");
            ShowHand();
            Console.Write("Select a card number to play: ");
            var line = Console.ReadLine();
            if (int.TryParse(line, out int cardIndex))
            {
                if (cardIndex >= Hand.Count || cardIndex < 0)
                {
                    Console.WriteLine("Please enter a valid index.");
                    MakeUserMove(playingTable);
                }
                /*
                 * User input is valid
                 */
                var cardToPlay = Hand[cardIndex];
                var cardToBeat = playingTable.LastCardToBeat();
                if (cardToBeat == null)
                {
                    /*
                     * User is attacking
                     */
                    MakeMoveAttack(cardToPlay, playingTable);
                }
                else
                {
                    /*
                     * User is defending
                     */                    
                    if ((cardToBeat.Suit == cardToPlay.Suit && cardToBeat.ValueInt < cardToPlay.ValueInt) ||
                        (cardToBeat.Suit != playingTable.TrumpSuit && cardToPlay.Suit == playingTable.TrumpSuit))
                    {
                        MakeMoveDefend(cardToPlay, playingTable);
                    }
                    else
                    {
                        Console.Write("You cannot make this move. ");
                        bool try_again = false;
                        foreach (Card checkCard in Hand)
                        {
                            if ((cardToBeat.Suit == checkCard.Suit && cardToBeat.ValueInt < checkCard.ValueInt) ||
                                (cardToBeat.Suit != playingTable.TrumpSuit && checkCard.Suit == playingTable.TrumpSuit))
                            {
                                try_again = true;
                                break;
                            }
                        }
                        if (try_again)
                        {
                            Console.WriteLine("Try another card.");
                            MakeUserMove(playingTable);
                        }
                        else
                        {
                            DefendFailure(playingTable);
                        }
                    }
                }                
            }
            else
            {
                Console.WriteLine("Please enter a valid index.");
                MakeUserMove(playingTable);
            }
        }

        public void MakeComputerMove(Table playingTable)
        {
            ShowHand();
            Console.Write($"Player {Name}'s move: ");            
            var handSorted = Hand.OrderBy(x => x.ValueInt).ToList();
            if (handSorted.Count != 0)
            {
                Card cardToPlay;
                if (playingTable.LastCardToBeat() == null)
                {
                    /*
                     *  Player is attacking
                     */
                    var handSortedNoTrump = handSorted.Where(x => x.Suit != playingTable.TrumpSuit).ToList();                    
                    if (handSortedNoTrump.Count != 0)
                    {
                        cardToPlay = handSortedNoTrump[0];
                    }
                    else
                    {
                        cardToPlay = handSorted[0];
                    }
                    MakeMoveAttack(cardToPlay, playingTable);
                }
                else
                {
                    /*
                     *  Player is defending
                     */
                    if (playingTable.LastCardToBeat().Suit == playingTable.TrumpSuit)
                    {
                        /*
                         *  Beating trump cards
                         */
                        var handSortedTrump = handSorted.Where(
                            x => x.Suit == playingTable.TrumpSuit && 
                                 x.ValueInt > playingTable.LastCardToBeat().ValueInt).ToList();
                        if (handSortedTrump.Count != 0)
                        {
                            MakeMoveDefend(handSortedTrump[0], playingTable);
                        }
                        else
                        {
                            DefendFailure(playingTable);
                        }
                    }
                    else
                    {
                        /*
                         *  Beating non-trump cards with same suit
                         */
                        var handSortedNoTrump = handSorted.Where(
                            x => x.Suit == playingTable.LastCardToBeat().Suit &&
                                 x.ValueInt > playingTable.LastCardToBeat().ValueInt).ToList();
                        if (handSortedNoTrump.Count != 0)
                        {
                            MakeMoveDefend(handSortedNoTrump[0], playingTable);
                        }
                        else
                        {
                            /*
                             *  Beating non-trump cards with a trump card
                             */
                            var handSortedTrump = handSorted.Where(x => x.Suit == playingTable.TrumpSuit).ToList();
                            if (handSortedTrump.Count != 0)
                            {
                                MakeMoveDefend(handSortedTrump[0], playingTable);
                            }
                            else
                            {
                                DefendFailure(playingTable);
                            }
                        }
                    }
                }                
            }
            else
            {
                Console.WriteLine($"Player {Name} has no more cards.");
            }
        }

        public void MakeMoveAttack(Card cardToPlay, Table playingTable)
        {
            Console.WriteLine(cardToPlay.ValueOf());
            Hand = Hand.Where(x => x.Id != cardToPlay.Id).ToList();
            playingTable.CardsToBeat.Add(new CardPair(cardToPlay));
        }

        public void MakeMoveDefend(Card cardToPlay, Table playingTable)
        {
            Console.WriteLine(cardToPlay.ValueOf());
            Hand = Hand.Where(x => x.Id != cardToPlay.Id).ToList();
            var cardPair = playingTable.LastCardToBeat();
            if (cardPair == null)
            {
                throw new Exception("Error: cardPair is null");
            }
            else
            {
                playingTable.BeatLastCard(cardToPlay);
            }
            Console.WriteLine("Defend successful");
            playingTable.Round++;
        }

        public void DefendFailure(Table playingTable)
        {
            foreach (CardPair cardPair in playingTable.CardsToBeat)
            {
                if (cardPair.Card1st == null)
                {
                    throw new Exception("Card1st in cardPair is null");
                }
                Hand.Add(cardPair.Card1st);
                if (cardPair.Card2nd != null)
                {
                    Hand.Add(cardPair.Card2nd);
                }
            }
            string message = "You take all cards";
            if (!IsUser)
            {
                message = $"Player {Name} takes all cards";
            }
            playingTable.Round++;
            Console.WriteLine(message);
        }

        /*
        public void MakeMoveSuccess(Card cardToPlay, Table playingTable)
        {
            Console.WriteLine(cardToPlay.ValueOf());
            Hand = Hand.Where(x => x.Id != cardToPlay.Id).ToList();
            if (Hand.Count == 0 && playingTable.TotalCards.Count == 0)
            {
                string message = "You won!";
                if (!IsUser)
                {
                    message = $"Player {Name} won!";
                }
                Console.WriteLine(message);
                playingTable.Players = playingTable.Players.Where(x => x.Id != Id).ToList();
            }
        }
        */
    }
}