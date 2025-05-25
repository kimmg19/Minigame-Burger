using UnityEngine;

public interface IDraggable
{
    bool CanDrag { get; }
    void OnDragStart();
    void OnDragEnd();
    string ItemName { get; }
}

public interface IDroppable
{
    bool CanAcceptDrop(IDraggable draggable);
    void OnItemDropped(IDraggable draggable);
    BurgerData GetBurgerData();
}