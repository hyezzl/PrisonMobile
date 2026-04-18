using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionTarget
{
    // 상호작용
    void Interact(PlayerInteractHandler player);
}
