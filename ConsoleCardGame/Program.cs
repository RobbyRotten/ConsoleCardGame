using System;

namespace ConsoleCardGame 
{    
    internal class Program
    {
        static void Main(string[] args)        {
                   
            var PlayingTable = new Table();
            // PlayingTable.ShowTotalCards();
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
                player.Hand = PlayingTable.GiveCard(6);
                // player.ShowHand();
                PlayingTable.Players.Add(player);
            }
            PlayingTable.ShowTrumpSuit();
            PlayingTable.SetPlayersPriority();            
            foreach (Player player in PlayingTable.Players)
            {
                player.MakeMove(PlayingTable);
            }            
        }
    }

    public class Table
    {
        public List<Player> Players = new List<Player>();
        public List<Card> TotalCards = new List<Card>();
        public string TrumpSuit;
        public Card? LastplayedCard;

        public Table() 
        {
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
                Console.WriteLine(Hand[cardIndex].ValueOf());
                playingTable.LastplayedCard = Hand[cardIndex];
                Hand.RemoveAt(cardIndex);
            }
            else
            {
                Console.WriteLine("Please enter a valid index.");
                MakeUserMove(playingTable);
            }
        }

        public void MakeComputerMove(Table playingTable)
        {
            Console.WriteLine($"Player {Name}'s move...");
            ShowHand();
            var handSorted = Hand.OrderBy(x => x.ValueInt).ToList();
            if (handSorted.Count != 0)
            {
                
                var handSortedNoTrump = handSorted.Where(x => x.Suit != playingTable.TrumpSuit).ToList();
                Card cardToPlay;
                if (handSortedNoTrump.Count != 0)
                {
                    cardToPlay = handSortedNoTrump[0];                    
                }
                else
                {
                    cardToPlay = handSorted[0];
                }
                Console.WriteLine(cardToPlay.ValueOf());
                Hand = Hand.Where(x => x.Id != cardToPlay.Id).ToList();
            }
            else
            {
                Console.WriteLine($"Player {Name} has no more cards.");
            }
        }
    }
}