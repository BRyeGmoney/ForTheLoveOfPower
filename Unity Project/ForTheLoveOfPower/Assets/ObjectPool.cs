using UnityEngine;
using System.Collections;
using System.Linq;
using AssemblyCSharp;

public class ObjectPool : MonoBehaviour {
    Settlement[] settlementPool;
    StructureUnit[] structurePool;
    MilitaryUnit[] unitPool;
    CombatIndicatorScript[] indicatorPool;

    public Object settlementDefault;
    public Object structureDefault;
    public Object unitDefault;
    public Object combatIndicatorDefault;

    public short SettlementPoolSize = 25;
    public short StructurePoolSize = 100;
    public short UnitPoolSize = 100;
    public short IndicatorPoolSize = 5;

    public GameObject[] unitObjects;
    public GameObject[] structureObjects;

    public RectTransform progressBar;
    public GameObject loadPanel;
    private short loaded;

    public static ObjectPool instance;

	// Use this for initialization
	void Awake() {
        instance = this;

        StartCoroutine(CreatePools());
	}

    IEnumerator CreatePools()
    {
        CreateSettlementPool();
        CreateStructurePool();
        CreateUnitPool();
        CreateIndicatorPool();

        StartCoroutine(SetGameGridInitState());

        yield return null;
    }

    void CreateSettlementPool()
    {
        settlementPool = new Settlement[SettlementPoolSize];

        for (int x = 0; x < SettlementPoolSize; x++)
        {
            settlementPool[x] = (Instantiate(settlementDefault) as GameObject).GetComponent<Settlement>();
            settlementPool[x].gameObject.SetActive(false);
            //AddToLoadBar();
        }
    }

    void CreateStructurePool()
    {
        structurePool = new StructureUnit[StructurePoolSize];

        for (int x = 0; x < StructurePoolSize; x++)
        {
            structurePool[x] = (Instantiate(structureDefault) as GameObject).GetComponent<StructureUnit>();
            structurePool[x].gameObject.SetActive(false);
            //AddToLoadBar();
        }
    }

    void CreateUnitPool()
    {
        unitPool = new MilitaryUnit[UnitPoolSize];

        for (int x = 0; x < UnitPoolSize; x++)
        {
            unitPool[x] = (Instantiate(unitDefault) as GameObject).GetComponent<MilitaryUnit>();
            unitPool[x].gameObject.SetActive(false);
            //AddToLoadBar();
        }
    }

    void CreateIndicatorPool()
    {
        indicatorPool = new CombatIndicatorScript[IndicatorPoolSize];

        for (int x = 0; x < IndicatorPoolSize; x++)
        {
            indicatorPool[x] = (Instantiate(combatIndicatorDefault) as GameObject).GetComponent<CombatIndicatorScript>();
            indicatorPool[x].gameObject.SetActive(false);
        }
    }

    #region GetNextAvailable
    MilitaryUnit GetNextAvailableUnit(int indexOfPlayer)
    {
        return unitPool.Skip(unitPool.Length / 2 * indexOfPlayer).Take(unitPool.Length / 2).First(unit => !unit.PoolObjInUse);
    }

    MilitaryUnit GetById(short id)
    {
        return unitPool.FirstOrDefault(unit => unit.ID == id);
    }

    Settlement GetSettlementById(short id)
    {
        return settlementPool.FirstOrDefault(settle => settle.ID == id);
    }

    StructureUnit GetStructureById(short id)
    {
        return structurePool.FirstOrDefault(struc => struc.ID == id);
    }

    StructureUnit GetNextAvailableStructure(int indexOfPlayer)
    {
        return structurePool.Skip(unitPool.Length / 2 * indexOfPlayer).Take(unitPool.Length / 2).First(structure => !structure.PoolObjInUse);
    }

    Settlement GetNextAvailableSettlement(int indexOfPlayer)
    {
        return settlementPool.Skip(unitPool.Length / 2 * indexOfPlayer).Take(unitPool.Length / 2).First(settlement => !settlement.PoolObjInUse);
    }

    CombatIndicatorScript GetNextAvailableIndicator()
    {
        return indicatorPool.First(indic => !indic.PoolObjInUse);
    }
    #endregion

    #region Creation
    public MilitaryUnit PullNewUnit(MilitaryUnitType desiredType, Vector3 desiredPosition, int indexOfPlayer, short id = -1)
    {
        MilitaryUnit newUnit;

        if (id < 0)
            newUnit = GetNextAvailableUnit(indexOfPlayer);
        else
            newUnit = GetById(id);

        if (desiredType.Equals(MilitaryUnitType.Dictator))
            SetupDictator(ref newUnit);
        else
            SetupMilitaryUnit((int)desiredType, ref newUnit);

        //set the unit's new position
        newUnit.transform.position = desiredPosition;

        //set the unit's inUse flag
        newUnit.PoolObjInUse = true;

        //last but not least, activate the object
        newUnit.gameObject.SetActive(true);

        return newUnit;
    }


    public StructureUnit PullNewStructure(StructureUnitType desiredType, Vector3 desiredPosition, int indexOfPlayer, short id = -1)
    {
        StructureUnit newStructure;

        if (id < 0)
            newStructure = GetNextAvailableStructure(indexOfPlayer);
        else
            newStructure = GetStructureById(id);

        SetupStructure((int)desiredType, ref newStructure);

        //set the structure's position
        newStructure.transform.position = desiredPosition;

        //set the structure's inUse flag
        newStructure.PoolObjInUse = true;

        //last but not least, activate the object
        newStructure.gameObject.SetActive(true);

        return newStructure;
    }

    public Settlement PullNewSettlement(Vector3 desiredPosition, int indexOfPlayer, short id = -1)
    {
        Settlement newSettlement;

        if (id < 0)
            newSettlement = GetNextAvailableSettlement(indexOfPlayer);
        else
            newSettlement = GetSettlementById(id);

        //set the settlement's new position
        newSettlement.transform.position = desiredPosition;

        //set the unit's inUse flag
        newSettlement.PoolObjInUse = true;

        //last but not least, activate the object
        newSettlement.gameObject.SetActive(true);

        return newSettlement;
    }

    public CombatIndicatorScript PullNewIndicator(Vector3 desiredPosition)
    {
        CombatIndicatorScript newIndicator = GetNextAvailableIndicator();

        //set the indicator's new position
        newIndicator.CombatPosition = desiredPosition;

        //set the indicator's inUse flag
        newIndicator.PoolObjInUse = true;

        //last but not least, activate the object
        newIndicator.gameObject.SetActive(true);

        return newIndicator;
    }
    #endregion

    #region Destruction
    public void DestroyOldUnit(MilitaryUnit unitToDestroy)
    {
        unitToDestroy.PoolObjInUse = false;
        unitToDestroy.gameObject.SetActive(false);
    }

    public void DestroyOldSettlement(Settlement settlementToDestroy)
    {
        settlementToDestroy.PoolObjInUse = false;
        settlementToDestroy.gameObject.SetActive(false);
    }

    public void DestroyOldStructure(StructureUnit structureToDestroy)
    {
        structureToDestroy.PoolObjInUse = false;
        structureToDestroy.gameObject.SetActive(false);
    }

    public void DestroyOldIndicator(CombatIndicatorScript indicatorToDestroy)
    {
        indicatorToDestroy.PoolObjInUse = false;
        indicatorToDestroy.gameObject.SetActive(false);
    }

    #endregion

    #region Units
    void SetupDictator(ref MilitaryUnit unitToSetup)
    {
        short tacticalShapeIndex = 2;

        unitToSetup.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = unitObjects[(int)MilitaryUnitType.Dictator].GetComponent<SpriteRenderer>().sprite;
        unitToSetup.gameObject.transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = unitObjects[(int)MilitaryUnitType.Dictator].transform.GetChild(tacticalShapeIndex).GetComponent<SpriteRenderer>().sprite;
    }

    void SetupMilitaryUnit(int milUnitTypeIndex, ref MilitaryUnit unitToSetup)
    {
        short tacticalShapeIndex = 4;

        unitToSetup.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = unitObjects[milUnitTypeIndex].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        unitToSetup.gameObject.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = unitObjects[milUnitTypeIndex].transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController;
        unitToSetup.gameObject.transform.GetChild(tacticalShapeIndex).GetComponent<SpriteRenderer>().sprite = unitObjects[milUnitTypeIndex].transform.GetChild(tacticalShapeIndex).GetComponent<SpriteRenderer>().sprite;
    }
    #endregion

    #region Structures
    void SetupStructure(int structTypeIndex, ref StructureUnit structToSetup)
    {
        structToSetup.GetComponent<SpriteRenderer>().sprite = structureObjects[structTypeIndex].GetComponent<SpriteRenderer>().sprite;
    }
    #endregion

    void AddToLoadBar()
    {
        progressBar.transform.localScale = new Vector3(loaded / (SettlementPoolSize + StructurePoolSize + UnitPoolSize), progressBar.transform.localScale.y, progressBar.transform.localScale.z);
        loaded++;
    }

    IEnumerator SetGameGridInitState()
    {
        yield return new WaitUntil(() => GameGridBehaviour.instance != null);
        //loadPanel.SetActive(false);
        //GameGridBehaviour.instance.InformDoneLoading();
    }
}
