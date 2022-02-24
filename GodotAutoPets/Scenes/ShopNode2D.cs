using System;
using System.Threading;
using Godot;
using AutoPets;

public class ShopNode2D : Node2D, IDragParent, ICardSlotDeck
{
    System.Threading.Thread _gameThread;

    public BuildNode BuildNode { get { return GetParent() as BuildNode; } }

    public Deck Deck { get { return BuildNode.Player.ShopDeck; } }

    [Signal]
    public delegate void CardBoughtSignal();

    public CardSlotNode2D GetCardSlotNode2D(int index)
    {
        return GetNode<CardSlotNode2D>(string.Format("CardSlotNode2D_{0}", index));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Dispose can be called from Godot editor so check if thread exists
        if (_gameThread != null)
            _gameThread.Abort();
    }

    public override void _Ready()
    {
        Connect("CardBoughtSignal", this, "_signal_CardBought", null, 
            (int)ConnectFlags.Deferred);
    }

    public void RenderShop()
    {
        for (int i = 0; i < Deck.Size; i++)
        {
            var card = Deck[i];
            var cardSlot = GetCardSlotNode2D(i + 1);
            cardSlot.CardArea2D.RenderCard(card, i, false);
            if (i >= GameSingleton.Instance.Game.ShopSlots)
                cardSlot.Hide();
        }
    }

    public override void _Process(float delta)
    {
        
    }

    // IDragParent
    public void DragDropped()
    {
        if (GameSingleton.Instance.DragTarget != null && GameSingleton.Instance.DragSource is CardArea2D)
        {
            var sourceCardArea2D = GameSingleton.Instance.DragSource as CardArea2D;
            var targetCardArea2D = GameSingleton.Instance.DragTarget;
            var sourceDeck = sourceCardArea2D.CardSlotNode2D.CardSlotDeck;
            var targetDeck = targetCardArea2D.CardSlotNode2D.CardSlotDeck;
            // did we drop onto the build deck?
            if (targetDeck.Deck == BuildNode.Player.BuildDeck)
            {
                // are we dropping onto an empty slot, or leveling up a card with same ability
                if (targetDeck.Deck[targetCardArea2D.CardIndex] == null ||
                    targetDeck.Deck[targetCardArea2D.CardIndex].Ability == sourceDeck.Deck[sourceCardArea2D.CardIndex].Ability)
                {
                    // select immediately before animations
                    targetCardArea2D.CardSlotNode2D.Selected = true;

                    // hide immediately since it's being dropped and animations are about 
                    // to be shown (e.g. if the bought card is buffed by an ability)
                    // we don't want the card shown in the shop during animations
                    sourceCardArea2D.HideCard();

                    _gameThread = new System.Threading.Thread(() => 
                    {
                        // from here events can be invoked in DeckNode2D, which send
                        // signals on main thread to render changes
                        GameSingleton.Instance.Game.BuyFromShop(sourceCardArea2D.CardIndex, targetCardArea2D.CardIndex, 
                            BuildNode.Player);
                        // notify the scene that the thread is finished
                        // assuming "this" is still valid. See Dispose method where thread is aborted
                        this.EmitSignal("CardBoughtSignal");
                    });
                    _gameThread.Name = "Shop Game Thread";
                    _gameThread.Start();
                }
            }
        }
    }

    public void DragReorder(CardArea2D cardArea2D)
    {

    }

    public bool GetCanDrag()
    {
        return BuildNode.Player.Gold >= Game.PetCost;
    }

    public void _signal_CardBought()
    {
        RenderShop();
        BuildNode.DeckNode2D.RenderDeck(BuildNode.DeckNode2D.Deck);
        BuildNode.DeckNode2D.PlayThump();
    }
}
