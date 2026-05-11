using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSaveInjector : MonoBehaviour
{
    private const string SaveFile = "KFSaveData.es3";
    private const string FishCollectionKey = "Player.FishCollection";

    [ContextMenu("Inject Fake Save Data")]
    public void InjectData()
    {
        List<FishSaveRecord> fakeSave = new List<FishSaveRecord>
        {
            new FishSaveRecord
            {
                Id = 1, // Assuming 1 is your Flamander in the CSV
                Discovered = true,
                NumberCaught = 5,
                LastWeight = 15.5f,
                LastSpeed = 10f
            }
        };

        ES3.Save(FishCollectionKey, fakeSave, SaveFile);
        Debug.Log("Fake Kaiju Save injected! Test ready.");
    }
}
