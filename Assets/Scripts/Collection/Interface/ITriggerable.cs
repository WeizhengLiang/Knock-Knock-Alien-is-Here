using UnityEngine;

public interface ITriggerable
{
    void OnTriggerStart(GameObject trigger);
    void OnTriggerEnd(GameObject trigger);
    void OnTriggerComplete(GameObject trigger);
    bool CanTrigger(GameObject trigger);
}