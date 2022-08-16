using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Haki {
    public class HakiLevel : LevelModule
    {
        public static HakiLevel local;
        public bool Nanomachines;
        public override IEnumerator OnLoadCoroutine()
        {
            local = this;
            return base.OnLoadCoroutine();
        }
    }
    public class HakiSpell : SpellCastCharge
    {
        public static PhysicMaterial hakiMaterial = new PhysicMaterial("Blade");
        public override void Fire(bool active)
        {
            base.Fire(active);
            if (active) {
                if (spellCaster.ragdollHand.gameObject.GetComponent<HakiRagdollHand>() == null)
                {
                    Creature.meshRaycast = false;
                    spellCaster.ragdollHand.gameObject.AddComponent<HakiRagdollHand>();
                    EffectInstance instance = Catalog.GetData<EffectData>("HakiEffect").Spawn(spellCaster.ragdollHand.transform, true);
                    instance.SetIntensity(1f);
                    instance.Play();
                }
                else if (spellCaster.ragdollHand.gameObject.GetComponent<HakiRagdollHand>() != null)
                {
                    if (spellCaster.ragdollHand.creature.gameObject.GetComponent<HakiRagdoll>() != null)
                        spellCaster.ragdollHand.creature.gameObject.GetComponent<HakiRagdoll>().DestroyThis();
                    spellCaster.ragdollHand.gameObject.GetComponent<HakiRagdollHand>().DestroyThis();
                }
            }
        }
        public override void Load(Imbue imbue, Level level)
        {
            base.Load(imbue, level);
            Item item = imbue.colliderGroup.GetComponentInParent<Item>();
            if (item.gameObject.GetComponent<HakiWeapon>() == null)
                item.gameObject.AddComponent<HakiWeapon>().Setup(imbue);
            else if (item.gameObject.GetComponent<HakiWeapon>() is HakiWeapon haki)
            {
                haki.UnimbueAndDestroy(imbue);
            }
        }
    }
    public class HakiSpellMerge : SpellMergeData
    {
        public override void Merge(bool active)
        {
            base.Merge(active);
            if (active && mana.casterLeft.ragdollHand.gameObject.GetComponent<HakiRagdollHand>() != null && mana.casterRight.ragdollHand.gameObject.GetComponent<HakiRagdollHand>() != null)
            {
                if(mana.creature.gameObject.GetComponent<HakiRagdoll>() is HakiRagdoll haki)
                {
                    Object.Destroy(haki);
                }
                else if(mana.creature.gameObject.GetComponent<HakiRagdoll>() == null)
                {
                    mana.creature.gameObject.AddComponent<HakiRagdoll>(); 
                }
            }
        }
    }
    public class HakiRagdollHand : MonoBehaviour
    {
        RagdollHand hand;
        Dictionary<Collider, PhysicMaterial> colliders = new Dictionary<Collider, PhysicMaterial>();
        Dictionary<Material, Color> colors = new Dictionary<Material, Color>();
        Dictionary<ColliderGroup, ColliderGroupData> groups = new Dictionary<ColliderGroup, ColliderGroupData>();
        public void Start()
        {
            hand = GetComponent<RagdollHand>();
        }

        public void Update()
        {
            foreach (Creature.RendererData renderer in hand.renderers)
            {
                if (!hand.otherHand.renderers.Contains(renderer))
                    foreach (Material material in renderer.renderer.materials)
                    {
                        if (!colors.ContainsKey(material)) colors.Add(material, material.GetColor("_BaseColor"));
                        material.SetColor("_BaseColor", Color.black);
                    }
            }
            if (!groups.ContainsKey(hand.colliderGroup)) groups.Add(hand.colliderGroup, hand.colliderGroup.data);
            hand.colliderGroup.data = Catalog.GetData<ColliderGroupData>("BladeMace2h");
            foreach (Collider collider in hand.colliderGroup.colliders)
            {
                if (!colliders.ContainsKey(collider)) colliders.Add(collider, collider.material);
                collider.material = HakiSpell.hakiMaterial;
            }
            if (hand.lowerArmPart.gameObject.GetComponent<HakiRagdollPart>() == null)
            {
                hand.lowerArmPart.gameObject.AddComponent<HakiRagdollPart>();
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
                            material.SetColor("_BaseColor", colors[material]);
                    }
            }
            foreach (Collider collider in hand.colliderGroup.colliders)
            {
                if (colliders.ContainsKey(collider))
                    collider.material = colliders[collider];
            }
            if (groups.ContainsKey(hand.colliderGroup))
                hand.colliderGroup.data = groups[hand.colliderGroup];
            if(hand.lowerArmPart.gameObject.GetComponent<HakiRagdollPart>() != null)
            {
                Destroy(hand.lowerArmPart.gameObject.GetComponent<HakiRagdollPart>());
            }
        }
    }
    public class HakiRagdollPart : MonoBehaviour
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
                collider.material = HakiSpell.hakiMaterial;
            }
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
        }
    }
    public class HakiRagdoll : MonoBehaviour
    {
        Creature creature;
        Dictionary<Material, Color> colors = new Dictionary<Material, Color>();
        EffectInstance instance;
        public void Start()
        {
            creature = GetComponent<Creature>();
            foreach (RagdollPart part in creature.ragdoll.parts)
            {
                if (part.gameObject.GetComponent<HakiRagdollPart>() == null || part.gameObject.GetComponent<HakiRagdollHand>() != null)
                    part.gameObject.AddComponent<HakiRagdollPart>();
            }
            foreach (Creature.RendererData renderer in creature.renderers)
            {
                foreach (Material material in renderer.renderer.materials)
                {
                    if (!colors.ContainsKey(material)) colors.Add(material, material.GetColor("_BaseColor"));
                    material.SetColor("_BaseColor", Color.black);
                }
            }
            if (HakiLevel.local.Nanomachines)
            {
                instance = Catalog.GetData<EffectData>("ItHasToBeThisWay").Spawn(creature.ragdoll.rootPart.transform, true);
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
                if (part != null && part.gameObject.GetComponent<HakiRagdollPart>() != null)
                    Destroy(part.gameObject.GetComponent<HakiRagdollPart>());
            }
            foreach (Creature.RendererData renderer in creature.renderers)
            {
                foreach (Material material in renderer.renderer.materials)
                {
                    if (colors.ContainsKey(material)) material.SetColor("_BaseColor", colors[material]);
                }
            }
            if (HakiLevel.local.Nanomachines)
                instance.Stop();
        }
    }
    public class HakiWeapon : MonoBehaviour
    {
        Item item; 
        Dictionary<Collider, PhysicMaterial> colliders = new Dictionary<Collider, PhysicMaterial>();
        Dictionary<Material, Color> colors = new Dictionary<Material, Color>();
        Dictionary<ColliderGroup, ColliderGroupData> groups = new Dictionary<ColliderGroup, ColliderGroupData>();
        Dictionary<DamageModifierData.Modifier, float> modifiersDamper = new Dictionary<DamageModifierData.Modifier, float>();
        Dictionary<DamageModifierData.Modifier, bool> modifiersPierce = new Dictionary<DamageModifierData.Modifier, bool>();
        bool isUnimbuing = false;
        public void Start()
        {
            item = GetComponent<Item>();
            foreach (ColliderGroup group in item.colliderGroups)
            {
                if (!groups.ContainsKey(group)) groups.Add(group, group.data);
                if (group.data.id != "CrystalStaff")
                group.data = Catalog.GetData<ColliderGroupData>("BladeSword2h");
            }
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                if (!colliders.ContainsKey(collider)) colliders.Add(collider, collider.material);
                collider.material = HakiSpell.hakiMaterial;
            }
            foreach (Renderer renderer in item.renderers)
            {
                if (!colors.ContainsKey(renderer.material)) colors.Add(renderer.material, renderer.material.GetColor("_BaseColor"));
                renderer.material.SetColor("_BaseColor", Color.black);
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
                    renderer.material.SetColor("_BaseColor", colors[renderer.material]);
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
            isUnimbuing = true;
            yield return new WaitForSeconds(2f);
            if (imbue.spellCastBase.GetType() == typeof(HakiSpell))
            {
                /*imbue.energy = 0;
                imbue.spellCastBase.Unload();
                imbue.spellCastBase = null;
                imbue.CancelInvoke();*/
                imbue.SetEnergyInstant(0);
            }
            Destroy(this);
            yield break;
        }
        public IEnumerator Unimbue(Imbue imbue)
        {
            yield return new WaitForSeconds(2f);
            if (imbue.spellCastBase.GetType() == typeof(HakiSpell))
            {
                /*imbue.energy = 0;
                imbue.spellCastBase.Unload();
                imbue.spellCastBase = null;
                imbue.CancelInvoke();*/
                imbue.SetEnergyInstant(0);
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
                        renderer.material.SetColor("_BaseColor", colors[renderer.material]);
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
            }
        }
    }
}
