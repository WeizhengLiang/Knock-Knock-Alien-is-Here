using UnityEngine;

public interface ITriggerable
{
    /// <summary>
    /// 检查是否可以触发交互
    /// </summary>
    /// <param name="trigger">触发器对象</param>
    /// <returns>true 表示可以触发交互，包括已解锁状态下的重复交互</returns>
    bool CanTrigger(GameObject trigger);

    /// <summary>
    /// 开始触发时的回调
    /// </summary>
    void OnTriggerStart(GameObject trigger);

    /// <summary>
    /// 结束触发时的回调
    /// </summary>
    void OnTriggerEnd(GameObject trigger);

    /// <summary>
    /// 完成触发时的回调
    /// 注意：即使在已解锁状态下也会调用此方法以支持重复交互
    /// </summary>
    void OnTriggerComplete(GameObject trigger);
}