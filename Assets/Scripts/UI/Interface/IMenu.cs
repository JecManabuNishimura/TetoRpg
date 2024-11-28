using UnityEngine;

public interface IMenu
{
    SelectView State { get; }
    void Horizontal(bool right);
    void Vertical(bool up);

    void Exit();
    void Entry();
    void Update();
    void SelectMenu();
    void CloseMenu();
}
public enum SelectView
{
    None,
    TabSelect,
    HaveMino,
    Armor,
}