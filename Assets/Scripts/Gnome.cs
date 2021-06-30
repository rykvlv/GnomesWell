using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gnome : MonoBehaviour
{
    public Transform cameraFollowTarget;

    public Rigidbody2D ropeBody;

    public Sprite armHoldingEmpty;
    public Sprite armHoldingTreasure;

    public SpriteRenderer holdingArm;

    public GameObject deathPrefab;
    public GameObject flameDeathPrefab;
    public GameObject ghostPrefab;

    public float delayBeforeRemoving = 3.0f;
    public float delayBeforeReleasingGhost = 0.25f;

    public GameObject bloodFountainPrefab;

    bool _isDead = false;
    bool _isHoldingTreasure = false;

    public bool isHoldingTreasure
    {
        get
        {
            return _isHoldingTreasure;
        }
        set
        {
            if (_isDead)
            {
                return;
            }
            _isHoldingTreasure = value;

            if (holdingArm != null)
            {
                if (_isHoldingTreasure)
                {
                    holdingArm.sprite = armHoldingTreasure;
                }
                else
                {
                    holdingArm.sprite = armHoldingEmpty;
                }
            }
        }
    }

    public enum DamageType
    {
        Slicing,
        Burning
    }

    public void ShowDamageEffect(DamageType type)
    {
        switch (type)
        {
            case DamageType.Burning:
                if (flameDeathPrefab != null)
                {
                    Instantiate(
                        flameDeathPrefab, cameraFollowTarget.position, cameraFollowTarget.rotation
                        );
                }
                break;
            case DamageType.Slicing:
                if (deathPrefab != null)
                {
                    Instantiate(
                        deathPrefab, cameraFollowTarget.position, cameraFollowTarget.rotation
                        );
                }
                break;
        }
    }

    public void DestroyGnome(DamageType type)
    {
        _isDead = true;
        isHoldingTreasure = false;

        foreach (BodyPart part in GetComponentsInChildren<BodyPart>())
        {
            switch (type)
            {
                case DamageType.Burning:
                    bool shouldBurn = Random.Range(0, 2) == 0;
                    if (shouldBurn)
                    {
                        part.ApplyDamageSprite(type);
                    }
                    break;
                case DamageType.Slicing:
                    part.ApplyDamageSprite(type);

                    break;
            }

            bool shouldDetach = Random.Range(0, 2) == 0;

            if (shouldDetach)
            {
                part.Detach();

                if (type == DamageType.Slicing)
                {
                    if (part.bloodFountainOrigin != null && bloodFountainPrefab != null)
                    {
                        GameObject fountain = Instantiate(
                            bloodFountainPrefab,
                            part.bloodFountainOrigin.position,
                            part.bloodFountainOrigin.rotation
                            ) as GameObject;

                        fountain.transform.SetParent(
                            this.cameraFollowTarget,
                            false
                            );
                    }
                }
            }

            var allJoints = part.GetComponentsInChildren<Joint2D>();
            foreach (Joint2D joint in allJoints)
            {
                Destroy(joint);
            }
        }
        var remove = gameObject.AddComponent<RemoveAfterDelay>();
        remove.delay = delayBeforeRemoving;
        StartCoroutine(ReleaseGhost());
    }

    IEnumerator ReleaseGhost()
    {
        if(ghostPrefab == null)
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeReleasingGhost);

        Instantiate(
            ghostPrefab,
            transform.position,
            Quaternion.identity
            );
    }
}

    
