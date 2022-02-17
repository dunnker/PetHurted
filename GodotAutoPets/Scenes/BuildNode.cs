using Godot;
using AutoPets;

public class BuildNode : Node
{
    public ShopNode2D Shop { get { return GetNode<ShopNode2D>("ShopNode2D"); } }

    public DeckNode2D Deck { get { return GetNode<global::DeckNode2D>("DeckNode2D"); } }

    public Label GoldLabel { get { return GetNode<Label>("PlayerAttrsNode2D/GoldLabel"); } }
    public Label LivesLabel { get { return GetNode<Label>("PlayerAttrsNode2D/LivesLabel"); } }
    public Label WinsLabel { get { return GetNode<Label>("PlayerAttrsNode2D/WinsLabel"); } }
    public Label RoundLabel { get { return GetNode<Label>("PlayerAttrsNode2D/RoundLabel"); } }
    public Label PlayerNameLabel { get { return GetNode<Label>("PlayerAttrsNode2D/PlayerNameLabel"); } } 

    public void _on_QuitGameButton_pressed()
    {
        GetTree().ChangeScene("res://Scenes/MainNode.tscn");
    }

    public void _on_RollButton_pressed()
    {
        if (GameSingleton.Instance.BuildPlayer.Gold >= Game.RollCost)
            GameSingleton.Instance.Game.Roll(GameSingleton.Instance.BuildPlayer);
        Shop.RenderShop();
    }

    public void _on_ContinueButton_pressed()
    {
        GameSingleton.Instance.BuildPlayer.GoldChangedEvent -= _GoldChangedEvent;
        if (GameSingleton.Instance.BuildPlayer == GameSingleton.Instance.Game.Player1)
        {
            GameSingleton.Instance.BuildPlayer = GameSingleton.Instance.Game.Player2;
            GetTree().ChangeScene("res://Scenes/BuildNode.tscn");
        }
        else
            GetTree().ChangeScene("res://Scenes/BattleNode.tscn");
    }
    
    public override void _Ready()
    {
        GameSingleton.Instance.BuildPlayer.GoldChangedEvent += _GoldChangedEvent;
        Deck.RenderDeck(GameSingleton.Instance.BuildPlayer.BuildDeck);
        GoldLabel.Text = GameSingleton.Instance.BuildPlayer.Gold.ToString();
        LivesLabel.Text = GameSingleton.Instance.BuildPlayer.Lives.ToString();
        WinsLabel.Text = GameSingleton.Instance.BuildPlayer.Wins.ToString();
        RoundLabel.Text = GameSingleton.Instance.Game.Round.ToString();
        PlayerNameLabel.Text = GameSingleton.Instance.BuildPlayer.Name;
    }

    public void _GoldChangedEvent(object sender, int oldValue)
    {
        GoldLabel.Text = GameSingleton.Instance.BuildPlayer.Gold.ToString();
    }
}