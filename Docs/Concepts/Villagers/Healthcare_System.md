# Healthcare & Medical Workforce System

**Status:** Concept  
**Category:** Villagers / Services  
**Created:** 2025-10-31  
**Last Updated:** 2025-10-31  
**Owner:** Design Team

---

## Summary

Villages sustain layered medical services ranging from roadside clinics to fully staffed hospitals. Entities seek treatment whenever their **Health drops below 90%**, paying fees that scale with injury severity, facility tier, and local outlooks. Severe injuries demand high-skill practitioners and specialized equipment; basic clinics may triage but refer to hospitals or itinerant surgeons when outmatched. Aggregate organizations (elite houses, armies, guilds, mercenary bands) often retain their own medical staff, bypassing wait times and shifting costs away from individuals.

---

## Player Experience Goals

- **Emotion:** Feel the tension between compassionate care and harsh triage economics.  
- **Fantasy:** Manage villages where healers, surgeons, herbalists, and miracle workers keep populations productive.  
- **Memorable Moment:** A wounded hero arrives with a shattered leg; the player must choose between expensive reconstruction using rare materials or quick amputation with lasting consequences.

---

## Core Mechanic

### Care Pipeline
1. **Assessment:** Entities flag injuries when Health ≤ 90% or when specific trauma (poison, plague, fractures) triggers.  
2. **Referral:** Smart agents evaluate available facilities (clinic, apothecary, hospital, temple healers, traveling surgeon) and travel to the best match they can afford or claim via affiliation perks.  
3. **Treatment Selection:** Highest-skill available medic at the location takes the case; treatment options unlock based on expertise, equipment, and supply inventories.  
4. **Outcome & Recovery:** Successful treatment restores Health, applies status adjustments (scar, prosthetic, immunity), and bills the paying party.

### Facility Tiers

| Facility | Staffing | Capability | Typical Patients |
|----------|----------|------------|------------------|
| **Clinic** | Apprentice medics | Minor wounds, stitches, basic diagnostics | Civilians with >70% Health |
| **Apothecary** | Herbalists | Poisons, plagues, tonic distribution | Villagers, beastmasters |
| **Hospital** | Surgeons, specialists | Major trauma, surgeries, prosthetics | Heroes, elites, army casualties |
| **Temple Infirmary** | Priests, miracle casters | Divine healing, curse removal | Devout followers, tithe payers |
| **Field Med-Tent** | Army medics | Combat triage, stabilization | Peacekeepers, mercenary bands |

Hospitals employ higher-skill individuals, sometimes importing specialists from outside the village. The **highest skill available medic** performs the procedure, optionally assisted by apprentices for speed.

---

## Treatment Options & Alternatives

| Injury | Standard Treatment | Alternative | Requirements |
|--------|--------------------|-------------|--------------|
| Fractured Limb | Bone-setting, splints, rehab | Amputation + prosthetic | High-quality materials, master surgeon |
| Severe Infection | Antidotes, herbal regimens | Divine cleansing | Rare herbs vs prayer power donation |
| Plague | Quarantine + apothecary tonics | Beastmaster diagnosis + druidic purge | Specialized expertise |
| Mortal Wound | Surgery + blood transfusion | Miracle intervention | Surgeons need sterile tools; miracles need high favor |

- **Saving a limb** requires expensive materials (mithril pins, healer sap, enchanted grafts); cost scales with rarity and alignment (evil villages may skimp).  
- **Amputation** needs minimal supplies but ideally anesthesia; skipping anesthesia inflicts trauma/debuffs.  
- **Miracle substitutes** consume prayer power but bypass material scarcity if the god intervenes.

---

## Employment & Patronage

- **Independent Clinics:** Serve general populace, charge per visit, may deny service if unpaid.  
- **Aggregate Employers:** Elite families, guilds, armies, and bands keep medics on retainer. Those medics prioritize their patrons before public requests.  
- **Village Subsidies:** Peacekeepers, militias, and armies are treated at village expense; budget strain affects taxes or morale.  
- **Traveling Specialists:** Rare, high-expertise doctors visit on schedules; players can court them with gifts or prestige.

---

## Economics & Alignment

- Treatment cost = `BaseFee × Severity × OutlookModifier`.  
- **Outlook/Alignment Modifiers:**  
  - Compassionate / Good villages discount civilian care but may tax elites to compensate.  
  - Ruthless / Evil settlements demand upfront payment, even for life-or-death care.  
  - Neutral pragmatists triage by productivity score (leaders, master artisans, soldiers).  
- Insurance-like guild contracts or army stipends automatically pay for their members.  
- Failing to pay reduces trust, leading to blacklists or debt peonage.

---

## Expertise & Staffing

- Medics have expertise tracks (Herbalism, Surgery, Diagnostics, Beastcare). Higher tiers improve treatment success and lesson quality for apprentices.  
- Hospitals dynamically assign cases: highest relevant expertise leads; others assist for XP gain.  
- Beastmasters with `AnimalsExpertise` can diagnose zoonotic plagues, accelerating containment.  
- Peacekeeper medics gain bonuses to rapid stabilization and are auto-funded by the settlement.

---

## Integration Hooks

- **Status Effects System:** Medical outcomes write scars, prosthetics, infections, or immunity statuses.  
- **Economy/Wealth Dynamics:** Clinics operate as businesses; hospitals may be communal projects.  
- **Faith & Miracles:** Temples can augment medical capacity at prayer-power cost.  
- **Event System:** Outbreaks, surgeon shortages, or malpractice cases trigger narrative events.  
- **Aggregate Entities:** Elite courts, armies, and guilds register health coverage policies affecting morale.

---

## Balancing Levers

1. **Skill Availability:** Limit top-tier medics to create scarcity and strategic protection.  
2. **Material Requirements:** Tune rare resource costs for advanced treatments.  
3. **Queue Times:** Busy hospitals create waitlists, motivating more clinics or divine intervention.  
4. **Outcome Risks:** Low-skill medics risk complications (infection, botched surgery), introducing drama.

---

## Open Questions

1. How do we visualize clinic/hospital capacity in the UI?  
2. Should anesthesia be its own resource (herbs, ether, divine calm)?  
3. How to handle cross-village medical tourism or black-market healers?

---
