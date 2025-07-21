using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastDirection = Vector2.right; // Default to facing right at start

    bool isBusy;

    public Transform indicator;
    //public Transform firePoint; // Reference to the fire point object
    //public float bulletForce;

    //GameObject focusedStation;

    //public Animator animator;

    void Update() 
    {
        if (!isBusy) 
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            if (movement != Vector2.zero)
            {
                lastDirection = movement.normalized; // Update the last direction and normalize it
            }

            // if (Input.GetButtonDown("Fire1")) 
            // {
            //     Fire();
            // }

            // if(Input.GetButtonDown("Fire2")) {
            //     Interact();
            // }
        } 
        else 
        {
            movement = Vector2.zero;
        }

        if (movement.sqrMagnitude > 1)
        {
            movement.Normalize();
        }

        //UpdateFirePoint(); // Keep the fire point in the correct position
        UpdateInteractionPoint();

        //animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.x < -0.01f) {
            GetComponent<SpriteRenderer>().flipX = false;
        } else if (movement.x > 0.01f) {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    void FixedUpdate() 
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void DisableMovement(bool state) 
    {
        isBusy = state;
        if (state) 
        {
            movement = Vector2.zero; // Ensure movement stops
           //animator.SetFloat("Speed", 0); // Stop animation from playing movement animations
        }
    }

    public bool GetIsBusy() {
        return isBusy;
    }

    // public void SetStationFocus(GameObject station) {
    //     focusedStation = station;
    // }

    // public GameObject GetStationFocus() {
    //     return focusedStation;
    // }

    // private void UpdateFirePoint() 
    // {
    //     // Rotate the fire point to match the last direction
    //     if (lastDirection != Vector2.zero) 
    //     {
    //         float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
    //         firePoint.rotation = Quaternion.Euler(0, 0, angle);
    //         indicator.rotation = Quaternion.Euler(0, 0, angle);
    //     }
    // }

    private void UpdateInteractionPoint() 
    {
        indicator.localPosition = new Vector3(lastDirection.x, lastDirection.y, 0).normalized;
    }

    // private void Fire() 
    // {
    //     if(GetComponent<PlayerStats>().bullets > 0) {
    //         GameObject bullet = ObjectPool.SharedInstance.GetPooledObject(); 
        
    //         if (bullet != null) {
    //             bullet.transform.position = firePoint.transform.position;
    //             bullet.transform.rotation = firePoint.transform.rotation;
    //             bullet.SetActive(true);
            
    //             Rigidbody2D bulletRB = bullet.GetComponent<Rigidbody2D>();
    //             Vector2 fireDirection = lastDirection.normalized;
    //             bulletRB.AddForce(fireDirection * bulletForce, ForceMode2D.Impulse);

    //             GetComponent<PlayerStats>().ReduceBullets();
    //             FindObjectOfType<AudioManager>().Play("PlayerShoot");
    //         }
    //     } else {
    //         if(timeBtwAttack <= 0) {
    //             Instantiate(slashPrefab, indicator.position, indicator.rotation);                
                
    //             Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(indicator.position, attackRange, whatIsEnemies);
    //             for (int i = 0; i < enemiesToDamage.Length; i++)
    //             {
    //                 enemiesToDamage[i].GetComponent<ITakeDamage>().TakeDamage(meleeDmg);
    //             }

    //             timeBtwAttack = startTimeBtwAttack;
    //         }
    //     }
    // }

    // void Interact() {
    //     if(focusedStation != null) {
    //         focusedStation.GetComponent<Station>().Interact();
    //     }
    // }
}
