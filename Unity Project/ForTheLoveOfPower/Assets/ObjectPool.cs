using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class ObjectPool : MonoBehaviour {
    Settlement[] settlementPool;
    StructureUnit[] structurePool;
    MilitaryUnit[] unitPool;

    public Object settlementDefault;
    public Object structureDefault;
    public Object unitDefault;

    public short SettlementPoolSize = 25;
    public short StructurePoolSize = 100;
    public short UnitPoolSize = 100;

    public RectTransform progressBar;
    public GameObject loadPanel;
    private short loaded;

	// Use this for initialization
	void Awake() {
        StartCoroutine(CreatePools());
	}

    IEnumerator CreatePools()
    {
        CreateSettlementPool();
        CreateStructurePool();
        CreateUnitPool();

        StartCoroutine(SetGameGridInitState());

        yield return null;
    }

    void CreateSettlementPool()
    {
        settlementPool = new Settlement[SettlementPoolSize];

        for (int x = 0; x < SettlementPoolSize; x++)
        {
            settlementPool[x] = (Instantiate(settlementDefault) as GameObject).GetComponent<Settlement>();
            //AddToLoadBar();
        }
    }

    void CreateStructurePool()
    {
        structurePool = new StructureUnit[StructurePoolSize];

        for (int x = 0; x < StructurePoolSize; x++)
        {
            structurePool[x] = (Instantiate(structureDefault) as GameObject).GetComponent<StructureUnit>();
            //AddToLoadBar();
        }
    }

    void CreateUnitPool()
    {
        unitPool = new MilitaryUnit[UnitPoolSize];

        for (int x = 0; x < UnitPoolSize; x++)
        {
            unitPool[x] = (Instantiate(unitDefault) as GameObject).GetComponent<MilitaryUnit>();
            //AddToLoadBar();
        }
    }

    void AddToLoadBar()
    {
        progressBar.transform.localScale = new Vector3(loaded / (SettlementPoolSize + StructurePoolSize + UnitPoolSize), progressBar.transform.localScale.y, progressBar.transform.localScale.z);
        loaded++;
    }

    IEnumerator SetGameGridInitState()
    {
        yield return new WaitUntil(() => GameGridBehaviour.instance != null);
        //loadPanel.SetActive(false);
        GameGridBehaviour.instance.InformDoneLoading();
    }
}
