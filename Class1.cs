using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ArmamentHaki
{
    public class MaterialProperties
    {
        public Color materialColor;
        public float materialSmoothness;
        public MaterialProperties(Color color, float smoothness)
        {
            materialColor = color;
            materialSmoothness = smoothness;
        }
    }
    public class ArmamentHakiLevel : ThunderScript
    {
        [ModOption(name: "Nanomachines, son", tooltip: "Becoming a United States senator has never been easier", valueSourceName: nameof(booleanOption), defaultValueIndex = 1)]
        public static bool Nanomachines = false;
        public static ModOptionBool[] booleanOption =
        {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };
    }
    public class ArmamentHakiSpell : SpellCastCharge
    {
        public static PhysicMaterial hakiMaterial = new PhysicMaterial("Blade");
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active) {
                if (spellCaster.ragdollHand.gameObject.GetComponent<ArmamentHakiRagdollHand>() == null)
                {
                    Creature.meshRaycast = false;
                    spellCaster.ragdollHand.gameObject.AddComponent<ArmamentHakiRagdollHand>();
                    EffectInstance instance = Catalog.GetData<EffectData>("ArmamentHakiEffect").Spawn(spellCaster.ragdollHand.transform, null, true);
                    instance.SetIntensity(1f);
                    instance.Play();
                }
                else if (spellCaster.ragdollHand.gameObject.GetComponent<ArmamentHakiRagdollHand>() != null)
                {
                    if (spellCaster.ragdollHand.creature.gameObject.GetComponent<ArmamentHakiRagdoll>() != null)
                        spellCaster.ragdollHand.creature.gameObject.GetComponent<ArmamentHakiRagdoll>().DestroyThis();
                    spellCaster.ragdollHand.gameObject.GetComponent<ArmamentHakiRagdollHand>().DestroyThis();
                }
            }
        }
        public override void Load(Imbue imbue, Level level)
        {
            base.Load(imbue, level);
            Item item = imbue.colliderGroup.GetComponentInParent<Item>();
            if (item.gameObject.GetComponent<ArmamentHakiWeapon>() == null)
                item.gameObject.AddComponent<ArmamentHakiWeapon>().Setup(imbue);
            else if (item.gameObject.GetComponent<ArmamentHakiWeapon>() is ArmamentHakiWeapon haki)
            {
                haki.UnimbueAndDestroy(imbue);
            }
        }
    }
    public class ArmamentHakiSpellMerge : SpellMergeData
    {
        public override void Merge(bool active)
        {
            base.Merge(active);
            if (active && mana.casterLeft.ragdollHand.gameObject.GetComponent<ArmamentHakiRagdollHand>() != null && mana.casterRight.ragdollHand.gameObject.GetComponent<ArmamentHakiRagdollHand>() != null)
            {
                if (mana.creature.gameObject.GetComponent<ArmamentHakiRagdoll>() is ArmamentHakiRagdoll haki)
                {
                    Object.Destroy(haki);
                }
                else if(mana.creature.gameObject.GetComponent<ArmamentHakiRagdoll>() == null)
                {
                    mana.creature.gameObject.AddComponent<ArmamentHakiRagdoll>(); 
                }
            }
        }
    }
    public class ArmamentHakiRagdollHand : MonoBehaviour
    {
        RagdollHand hand;
        Dictionary<Collider, PhysicMaterial> colliders = new Dictionary<Collider, PhysicMaterial>();
        Dictionary<Material, MaterialProperties> colors = new Dictionary<Material, MaterialProperties>();
        Dictionary<ColliderGroup, ColliderGroupData> groups = new Dictionary<ColliderGroup, ColliderGroupData>();
        public void Start()
        {
            hand = GetComponent<RagdollHand>();
            hand.creature.OnDamageEvent += Creature_OnDamageEvent;
        }

        private void Creature_OnDamageEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && collisionInstance.targetColliderGroup == hand.colliderGroup) collisionInstance.damageStruct.damage = 0;
        }

        public void Update()
        {
            foreach (Creature.RendererData renderer in hand.renderers)
            {
                if (!hand.otherHand.renderers.Contains(renderer))
                    foreach (Material material in renderer.renderer.materials)
                    {
                        if (!colors.ContainsKey(material)) colors.Add(material, new MaterialProperties(material.GetColor("_BaseColor"), material.GetFloat("_Smoothness")));
                        material.SetColor("_BaseColor", Color.black);
                        material.SetFloat("_Smoothness", 1);
                    }
            }
            if (!groups.ContainsKey(hand.colliderGroup)) groups.Add(hand.colliderGroup, hand.colliderGroup.data);
            hand.colliderGroup.data = Catalog.GetData<ColliderGroupData>("BladeMace2h");
            foreach (Collider collider in hand.colliderGroup.colliders)
            {
                if (!colliders.ContainsKey(collider)) colliders.Add(collider, collider.material);
                collider.material = ArmamentHakiSpell.hakiMaterial;
            }
            if (hand.lowerArmPart.gameObject.GetComponent<ArmamentHakiRagdollPart>() == null)
            {
                hand.lowerArmPart.gameObject.AddComponent<ArmamentHakiRagdollPart>();
            }
        }
        public void DestroyThis()
        {
            Destroy(this);
        }
        public void OnDestroy()
        {
            foreach (Creature.RendererData renderer in hand.renderers)
            {
                if (!hand.otherHand.renderers.Contains(renderer))
                    foreach (Material material in renderer.renderer.materials)
                    {
                        if (colors.ContainsKey(material))
                        {
                            material.SetColor("_BaseColor", colors[material].materialColor);
                            material.SetFloat("_Smoothness", colors[material].materialSmoothness);
                        }
                    }
            }
            foreach (Collider collider in hand.colliderGroup.colliders)
            {
                if (colliders.ContainsKey(collider))
                    collider.material = colliders[collider];
            }
            if (groups.ContainsKey(hand.colliderGroup))
                hand.colliderGroup.data = groups[hand.colliderGroup];
            if(hand.lowerArmPart.gameObject.GetComponent<ArmamentHakiRagdollPart>() != null)
            {
                Destroy(hand.lowerArmPart.gameObject.GetComponent<ArmamentHakiRagdollPart>());
            }
            hand.creature.OnDamageEvent -= Creature_OnDamageEvent;
        }
    }
    public class ArmamentHakiRagdollPart : MonoBehaviour
    {
        RagdollPart part;
        Dictionary<Collider, PhysicMaterial> colliders = new Dictionary<Collider, PhysicMaterial>();
        Dictionary<ColliderGroup, ColliderGroupData> groups = new Dictionary<ColliderGroup, ColliderGroupData>();
        public void Start()
        {
            part = GetComponent<RagdollPart>();
            if (!groups.ContainsKey(part.colliderGroup)) groups.Add(part.colliderGroup, part.colliderGroup.data);
            part.colliderGroup.data = Catalog.GetData<ColliderGroupData>("BladeMace2h");
            foreach (Collider collider in part.colliderGroup.colliders)
            {
                if (!colliders.ContainsKey(collider)) colliders.Add(collider, collider.material);
                collider.material = ArmamentHakiSpell.hakiMaterial;
            }
            part.ragdoll.creature.OnDamageEvent += Creature_OnDamageEvent;
        }

        private void Creature_OnDamageEvent(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart && collisionInstance.targetColliderGroup == part.colliderGroup) collisionInstance.damageStruct.damage = 0;
        }

        public void OnDestroy()
        {
            if (part.colliderGroup != null)
            {
                foreach (Collider collider in part.colliderGroup.colliders)
                {
                    if (collider != null && colliders.ContainsKey(collider))
                        collider.material = colliders[collider];
                }
                if (groups.ContainsKey(part.colliderGroup))
                    part.colliderGroup.data = groups[part.colliderGroup];
            }
            part.ragdoll.creature.OnDamageEvent -= Creature_OnDamageEvent;
        }
    }
    public class ArmamentHakiRagdoll : MonoBehaviour
    {
        Creature creature;
        Dictionary<Material, MaterialProperties> colors = new Dictionary<Material, MaterialProperties>();
        EffectInstance instance;
        public void Start()
        {
            creature = GetComponent<Creature>();
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (part.gameObject.GetComponent<ArmamentHakiRagdollPart>() == null && part.gameObject.GetComponent<ArmamentHakiRagdollHand>() == null)
                    part.gameObject.AddComponent<ArmamentHakiRagdollPart>();
            }
            foreach (Creature.RendererData renderer in creature.renderers)
            {
                foreach (Material material in renderer.renderer.materials)
                {
                    if (!colors.ContainsKey(material)) colors.Add(material, new MaterialProperties(material.GetColor("_BaseColor"), material.GetFloat("_Smoothness")));
                    material.SetColor("_BaseColor", Color.black);
                    material.SetFloat("_Smoothness", 1);
                }
            }
            if (ArmamentHakiLevel.Nanomachines)
            {
                instance = Catalog.GetData<EffectData>("ItHasToBeThisWay").Spawn(creature.ragdoll.rootPart.transform, null, true);
                instance.SetIntensity(1f);
                instance.Play();
            }
        }
        public void DestroyThis()
        {
            Destroy(this);
        }
        public void OnDestroy()
        {
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (part != null && part.gameObject.GetComponent<ArmamentHakiRagdollPart>() != null)
                    Destroy(part.gameObject.GetComponent<ArmamentHakiRagdollPart>());
            }
            foreach (Creature.RendererData renderer in creature.renderers)
            {
                foreach (Material material in renderer.renderer.materials)
                {
                    if (colors.ContainsKey(material))
                    {
                        material.SetColor("_BaseColor", colors[material].materialColor);
                        material.SetFloat("_Smoothness", colors[material].materialSmoothness);
                    }
                }
            }
            if (instance != null)
                instance.Stop();
        }
    }
    public class ArmamentHakiWeapon : MonoBehaviour
    {
        Item item; 
        Dictionary<Collider, PhysicMaterial> colliders = new Dictionary<Collider, PhysicMaterial>();
        Dictionary<Material, MaterialProperties> colors = new Dictionary<Material, MaterialProperties>();
        Dictionary<ColliderGroup, ColliderGroupData> groups = new Dictionary<ColliderGroup, ColliderGroupData>();
        Dictionary<DamageModifierData.Modifier, float> modifiersDamper = new Dictionary<DamageModifierData.Modifier, float>();
        Dictionary<DamageModifierData.Modifier, bool> modifiersPierce = new Dictionary<DamageModifierData.Modifier, bool>();
        bool isUnimbuing = false;
        Breakable breakable;
        bool instantBreak;
        float threshold;
        public void Start()
        {
            item = GetComponent<Item>();
            breakable = item.GetComponent<Breakable>();
            if (breakable != null)
            {
                breakable?.onTakeDamage?.AddListener(OnTakeDamage);
                instantBreak = breakable.canInstantaneouslyBreak;
                threshold = breakable.instantaneousBreakVelocityThreshold;
                breakable.canInstantaneouslyBreak = false;
                breakable.instantaneousBreakVelocityThreshold = Mathf.Infinity;
            }
            foreach (ColliderGroup group in item.colliderGroups)
            {
                if (!groups.ContainsKey(group)) groups.Add(group, group.data);
                if (group.data.id != "CrystalStaff")
                group.data = Catalog.GetData<ColliderGroupData>("BladeSword2h");
            }
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                if (!colliders.ContainsKey(collider)) colliders.Add(collider, collider.material);
                collider.material = ArmamentHakiSpell.hakiMaterial;
            }
            foreach (Renderer renderer in item.renderers)
            {
                if (!colors.ContainsKey(renderer.material)) colors.Add(renderer.material, new MaterialProperties(renderer.material.GetColor("_BaseColor"), renderer.material.GetFloat("_Smoothness")));
                renderer.material.SetColor("_BaseColor", Color.black);
                renderer.material.SetFloat("_Smoothness", 1);
            }
            foreach (Damager damager in item.GetComponentsInChildren<Damager>())
            {
                foreach(DamageModifierData.Collision collision in damager.data.damageModifierData.collisions)
                {
                    foreach (DamageModifierData.Modifier modifier in collision.modifiers)
                    {
                        if (!modifiersDamper.ContainsKey(modifier)) modifiersDamper.Add(modifier, modifier.penetrationDamperMultiplier);
                        modifier.penetrationDamperMultiplier = 0;
                        if (damager.data.damageModifierData.damageType != DamageType.Blunt)
                        {
                            if (!modifiersPierce.ContainsKey(modifier)) modifiersPierce.Add(modifier, modifier.allowPenetration);
                            modifier.allowPenetration = true;
                        }
                    }
                }
            }
        }
        public void OnTakeDamage(float sqrMagnitude)
        {
            ++breakable.hitsUntilBreak;
        }

        public void Setup(Imbue imbue)
        {
            StartCoroutine(Unimbue(imbue));
        }
        public void UnimbueAndDestroy(Imbue imbue)
        {
            StartCoroutine(UnimbueThenDelete(imbue));
        }
        public IEnumerator UnimbueThenDelete(Imbue imbue)
        {
            foreach (ColliderGroup group in item.colliderGroups)
            {
                if (groups.ContainsKey(group) && group.data.id != "CrystalStaff") group.data = groups[group];
            }
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                if (colliders.ContainsKey(collider))
                    collider.material = colliders[collider];
            }
            foreach (Renderer renderer in item.renderers)
            {
                if (colors.ContainsKey(renderer.material))
                {
                    renderer.material.SetColor("_BaseColor", colors[renderer.material].materialColor);
                    renderer.material.SetFloat("_Smoothness", colors[renderer.material].materialSmoothness);
                }
            }
            foreach (Damager damager in item.GetComponentsInChildren<Damager>())
            {
                foreach (DamageModifierData.Collision collision in damager.data.damageModifierData.collisions)
                {
                    foreach (DamageModifierData.Modifier modifier in collision.modifiers)
                    {
                        if (modifiersDamper.ContainsKey(modifier)) modifier.penetrationDamperMultiplier = modifiersDamper[modifier];
                        if (damager.data.damageModifierData.damageType != DamageType.Blunt)
                            if (modifiersPierce.ContainsKey(modifier)) modifier.allowPenetration = modifiersPierce[modifier];
                    }
                }
            }
            if (breakable != null)
            {
                breakable?.onTakeDamage?.RemoveListener(OnTakeDamage);
                breakable.canInstantaneouslyBreak = instantBreak;
                breakable.instantaneousBreakVelocityThreshold = threshold;
            }
            isUnimbuing = true;
            yield return new WaitForSeconds(2f);
            if (imbue.spellCastBase.GetType() == typeof(ArmamentHakiSpell))
            {
                imbue.Stop();
            }
            Destroy(this);
            yield break;
        }
        public IEnumerator Unimbue(Imbue imbue)
        {
            yield return new WaitForSeconds(2f);
            if (imbue.spellCastBase.GetType() == typeof(ArmamentHakiSpell))
            {
                imbue.Stop();
            }
            yield break;
        }
        public void DestroyThis()
        {
            Destroy(this);
        }
        public void OnDestroy()
        {
            if (!isUnimbuing)
            {
                foreach (ColliderGroup group in item.colliderGroups)
                {
                    if (groups.ContainsKey(group) && group.data.id != "CrystalStaff") group.data = groups[group];
                }
                foreach (Collider collider in item.GetComponentsInChildren<Collider>())
                {
                    if (colliders.ContainsKey(collider))
                        collider.material = colliders[collider];
                }
                foreach (Renderer renderer in item.renderers)
                {
                    if (colors.ContainsKey(renderer.material))
                    {
                        renderer.material.SetColor("_BaseColor", colors[renderer.material].materialColor);
                        renderer.material.SetFloat("_Smoothness", colors[renderer.material].materialSmoothness);
                    }
                }
                foreach (Damager damager in item.GetComponentsInChildren<Damager>())
                {
                    foreach (DamageModifierData.Collision collision in damager.data.damageModifierData.collisions)
                    {
                        foreach (DamageModifierData.Modifier modifier in collision.modifiers)
                        {
                            if (modifiersDamper.ContainsKey(modifier)) modifier.penetrationDamperMultiplier = modifiersDamper[modifier];
                            if (damager.data.damageModifierData.damageType != DamageType.Blunt)
                                if (modifiersPierce.ContainsKey(modifier)) modifier.allowPenetration = modifiersPierce[modifier];
                        }
                    }
                }
                if (breakable != null)
                {
                    breakable?.onTakeDamage?.RemoveListener(OnTakeDamage);
                    breakable.canInstantaneouslyBreak = instantBreak;
                    breakable.instantaneousBreakVelocityThreshold = threshold;
                }
            }
        }
    }
}
