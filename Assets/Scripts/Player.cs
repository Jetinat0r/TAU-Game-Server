using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public float maxHealth = 100f;
    private float health = 100f;

    public bool isAlive = true;

    //0: None
    //1: Pistol
    //2: Shotgun
    public int curWeapon;

    //0: Pseudo-Innocent, used to not count people joining mid-round
    //1: Innocent
    //2: Traitor
    public int gameRole;

    //Only need the rotation for the weapon pivot
    public Quaternion pivotRotation;

    public Color playerColor;

    [Header("Weapon Stats")]
    [SerializeField]
    private List<Weapon> weapons;

    [HideInInspector]
    public int completedTasks = 0;
    [HideInInspector]
    public int totalTasks = 0;

    //MAY NEED TO BE FIXED UPDATE
    private void FixedUpdate()
    {
        SendMovement();
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        curWeapon = 0;
        gameRole = 0;
        playerColor = GameManager.instance.playerColors[_id - 1];
        //position = _spawnPosition;
        pivotRotation = Quaternion.identity;

        GetComponent<SpriteRenderer>().color = playerColor;
    }

    private void SendMovement()
    {
        //Can be horribly hacked, idc
        ServerSend.PlayerMovement(this);
    }

    public void SetPosRot(Vector3 _position, Quaternion _rotation)
    {
        transform.position = _position;
        pivotRotation = _rotation;
    }

    public void WeaponSwap(int _weapon)
    {
        curWeapon = _weapon;

        ServerSend.RemoteWeaponSwap(id, curWeapon);
    }

    public void Shoot(Vector3 _position, Quaternion _rotation, float _playerSpeed)
    {
        Weapon _heldWeapon = weapons[curWeapon];

        for (int i = 0; i < _heldWeapon.numShots; i++)
        {
            //SET NEW ROTATION FOR BULLAT UP HERE, JUST IN CASE
            Vector3 _angle = _rotation.eulerAngles;
            _angle.z += UnityEngine.Random.Range(-_heldWeapon.shotAngle, _heldWeapon.shotAngle);

            CreateBullet(_heldWeapon, _position, _angle, _playerSpeed);
        }
    }

    private void CreateBullet(Weapon _heldWeapon, Vector3 _position, Vector3 _angle, float _playerSpeed)
    {
        GameObject _curBullet = Instantiate(_heldWeapon.bullet, _position, Quaternion.Euler(_angle));
        _curBullet.name = _heldWeapon.name + " Bullet";

        Bullet _bulletScript = _curBullet.GetComponent<Bullet>();

        _bulletScript.velocity = _heldWeapon.velocity * UnityEngine.Random.Range(1 - _heldWeapon.velocityRandomizer, 1 + _heldWeapon.velocityRandomizer) * _playerSpeed;
        _bulletScript.damage = _heldWeapon.damage;
        _bulletScript.shotPosOffset = _curBullet.transform.right.normalized * UnityEngine.Random.Range(0f, _heldWeapon.shotOffset);


        ServerSend.SpawnBullet(_curBullet.transform.position, _curBullet.transform.rotation, _bulletScript.velocity, _bulletScript.shotPosOffset);
    }

    public void TakeDamage(float _amount)
    {
        health -= _amount;

        ServerSend.DamagePlayer(id, _amount);

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void Die()
    {
        isAlive = false;

        int _layer = LayerMask.NameToLayer("Ghost");
        gameObject.layer = _layer;

        ServerSend.RemoteDeath(id, 0);
    }

    public void Resurrect()
    {
        isAlive = true;
        health = maxHealth;

        int _layer = LayerMask.NameToLayer("Default");
        gameObject.layer = _layer;

        //ServerSend.Resurrect();
    }

    public void AssignTasks(int _numTasks)
    {
        if(gameRole == 1)
        {
            completedTasks = 0;
            totalTasks = _numTasks;
        }
        else
        {
            completedTasks = -1;
            totalTasks = -1;
        }
        
    }
}