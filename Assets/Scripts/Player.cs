using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    Habitat _location = Habitat.Normal;

    public Habitat _GetCurrentHabitat { get { return _location; } }

}
