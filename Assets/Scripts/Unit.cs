using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public bool selected;
    GameMaster gm;

    public int tileSpeed;
    public bool hasMoved;

    public float moveSpeed;

    public int playerNumber;

    public int attackRange;
    public List<Unit> enemiesInRange = new List<Unit>();
    public bool hasAttacked;

    public GameObject weaponIcon;

    public int health;
    public int attackDamage;
    public int defenseDamage;
    public int armour;

    public DamageIcon damageIcon;

    public GameObject deathEffect;

    private Animator camAnim;

    private AudioSource source;
    //public AudioClip selectedSound;
    //public AudioClip moveSound;

    public Text kingHealth;
    public bool isKing;

    public GameObject victoryPanel;

    void Start()
    {
        source = GetComponent<AudioSource>();
        gm = FindObjectOfType<GameMaster>();
        camAnim = Camera.main.GetComponent<Animator>();
        UpdateKingHealth();
    }

    public void UpdateKingHealth()
    {
        if (isKing == true)
        {
            kingHealth.text = health.ToString();
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) //Right Click
        {
            gm.ToggleStatsPanel(this);
        }
    }

    void OnMouseDown()
    {
        ResetWeaponIcon();

        if (selected == true)
        {
            //unselect unit
            selected = false;
            gm.selectedUnit = null;
            gm.ResetTiles();
            ResetWeaponIcon();

        }else
        {

            if (playerNumber == gm.playerTurn)
            {
                //unselect another unit that is already selected
                if (gm.selectedUnit != null)
                {
                    gm.selectedUnit.selected = false;
                }
                //source.clip = selectedSound;
                source.Play();
                selected = true;
                gm.selectedUnit = this;

                gm.ResetTiles();
                GetEnemies();
                GetWalkableTiles();
            }
        }

        Collider2D col = Physics2D.OverlapCircle(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.15f);

        if (col != null)
        {
            Unit unit = col.GetComponent<Unit>(); //Double check we clicked on a unit

            if (unit != null && gm.selectedUnit != null)
            {
                if (gm.selectedUnit.enemiesInRange.Contains(unit) && gm.selectedUnit.hasAttacked == false)
                { //Does the currently selected unit have in his list the enemy we just clicked on
                    gm.selectedUnit.Attack(unit);
                }
            }
        }
        
    }

    void Attack(Unit enemy)
    {
        camAnim.SetTrigger("shake");

        hasAttacked = true;

        int enemyDamage = attackDamage - enemy.armour;
        int myDamage = enemy.defenseDamage - armour;

        if (enemyDamage >= 1)
        {
            DamageIcon instance = Instantiate(damageIcon, enemy.transform.position, Quaternion.identity);
            instance.Setup(enemyDamage);
            enemy.health -= enemyDamage;
            enemy.UpdateKingHealth();
        }

        if (transform.tag == "Archer" && enemy.tag != "Archer")
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= 1)
            {
                if (myDamage >= 1)
                {
                    DamageIcon instance = Instantiate(damageIcon, transform.position, Quaternion.identity);
                    instance.Setup(myDamage);
                    health -= myDamage;
                    UpdateKingHealth();
                }
            }
        }
        else
        {
            if (myDamage >= 1)
            {
                DamageIcon instance = Instantiate(damageIcon, transform.position, Quaternion.identity);
                instance.Setup(myDamage);
                health -= myDamage;
                UpdateKingHealth();
            }
        }



        if (enemy.health <= 0)
        {
            if (enemy.isKing == true)
            {
                enemy.victoryPanel.SetActive(true);
            }

            Instantiate(deathEffect, enemy.transform.position, Quaternion.identity);
            Destroy(enemy.gameObject);
            GetWalkableTiles();
            gm.RemoveStatsPanel(enemy);
        }

        if (health <= 0)
        {

            if (isKing == true)
            {
                victoryPanel.SetActive(true);
            }

            Instantiate(deathEffect, transform.position, Quaternion.identity);
            gm.ResetTiles();
            gm.RemoveStatsPanel(this);
            Destroy(this.gameObject);
        }

        gm.UpdateStatsPanel();
    }

    void GetWalkableTiles()
    {
        if (hasMoved == true)
        {
            return;
        }

        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            if (Mathf.Abs(transform.position.x - tile.transform.position.x) + Mathf.Abs(transform.position.y - tile.transform.position.y) <= tileSpeed)
            {
                if (tile.IsClear() == true)
                {
                    tile.Highlight();
                }
            }
        }
    }

    void GetEnemies()
    {
        enemiesInRange.Clear();

        Unit[] enemies = FindObjectsOfType<Unit>();

        foreach (Unit enemy in enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= attackRange) // Check is the enemy near enough to attack
            {
                if (enemy.playerNumber != gm.playerTurn && hasAttacked == false) // Make sure you cant attack allies
                {
                    enemiesInRange.Add(enemy);
                    enemy.weaponIcon.SetActive(true);
                }
            }
        }
    }

    public void ResetWeaponIcon()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            unit.weaponIcon.SetActive(false);
        }
    }

    public void Move(Vector2 tilePos)
    {
        gm.ResetTiles();
        StartCoroutine(StartMovement(tilePos));
    }

    IEnumerator StartMovement(Vector2 tilePos)
    {

        //source.clip = moveSound;
        //source.Play();

        while(transform.position.x != tilePos.x)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(tilePos.x, transform.position.y), moveSpeed * Time.deltaTime);
            yield return null;
        }

        while (transform.position.y != tilePos.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, tilePos.y), moveSpeed * Time.deltaTime);
            yield return null;
        }

        hasMoved = true;
        ResetWeaponIcon();
        GetEnemies();
        gm.MoveStatsPanel(this);
    }
}
