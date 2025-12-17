# Godgame Conceptualization Repository

Living collection of game design ideas and mechanics (**what the game should be**). Implementation details live in the Truth Source docs.

- Active categories: `Buildings/`, `Combat/`, `Core/`, `Economy/`, `Experiences/`, `Interaction/`, `Meta/`, `Miracles/`, `Progression/`, `Resources/`, `UI_UX/`, `Villagers/`, `World/`.
- Templates: `Docs/Concepts/_Templates/Feature.md`, `Mechanic.md`, `System.md`, `Experience.md`.
- Legacy/archived drafts are now under `Archive/Concepts_legacy/`. Use them only for historical reference.

## How to add a concept
- Pick a category folder and start a markdown doc; tag the top with a status (`Status: Draft`, `In Review`, `Approved`, `In Development`, `Implemented`, `On Hold`, `Archived`). See `Docs/Concepts/WIP_FLAGS.md` for suggested flags.
- For fuller designs, copy a template from `_Templates/` and link to related concepts and Truth Sources.

## Relationship to Truth Sources
Concepts describe intent; Truth Sources define DOTS contracts and runtime systems. When a concept becomes concrete, capture the contract in `Docs/TruthSources_*.md` and implement under `Assets/Scripts/Godgame` or `Packages/com.moni.puredots`.
1. **Performance** - Must run 1000+ villagers smoothly
2. **Scope** - MVP first, creature/multiplayer later
3. **Clarity** - Near-HUD-less UI, visual feedback over text
4. **Accessibility** - Readable UI, colorblind support
5. **Moddability** - Data-driven design for future modding

---

## 📊 Concept Status Dashboard

*Updated manually as concepts are added*

| Category | Draft | In Review | Approved | Implemented | Total |
|----------|-------|-----------|----------|-------------|-------|
| Core | 1 | 1 | 0 | 0 | 2 |
| Villagers | 2 | 0 | 0 | 0 | 2 |
| Resources | 0 | 0 | 1 | 0 | 1 |
| Buildings | 1 | 0 | 1 | 0 | 2 |
| Interaction | 0 | 0 | 3 | 0 | 3 |
| Experiences | 0 | 0 | 1 | 0 | 1 |
| Miracles | 1 | 0 | 0 | 0 | 1 |
| Combat | 0 | 0 | 0 | 0 | 0 |
| Creature | 0 | 0 | 0 | 0 | 0 |
| World | 1 | 0 | 0 | 0 | 1 |
| Progression | 1 | 0 | 0 | 0 | 1 |
| Politics | 3 | 1 | 0 | 0 | 4 |
| UI/UX | 0 | 0 | 1 | 0 | 1 |
| Meta | 2 | 0 | 0 | 0 | 2 |

---

## 🔗 Related Documentation

- **Truth Sources** - `Docs/TruthSources_Inventory.md` - Technical implementation
- **Architecture** - `Docs/TruthSources_Architecture.md` - DOTS patterns  
- **Legacy Salvage** - `Docs/Legacy_TruthSources_Salvage.md` - Preserved contracts
- **Legacy Porting Guide** - `Docs/Concepts/LEGACY_PORTING_GUIDE.md` - How to port
- **Legacy Port Status** - `Docs/Concepts/LEGACY_PORT_STATUS.md` - Progress tracker
- **Porting Summary** - `Docs/Concepts/PORTING_SUMMARY.md` - Overview
- **WIP Flags** - `Docs/Concepts/WIP_FLAGS.md` - Uncertainty markers
- **Integration TODO** - `Docs/TODO/Godgame_PureDOTS_Integration_TODO.md` - Work tracking

---

## 💡 Tips for Writing Good Concepts

### DO ✅
- **Be specific** - "Swordsmen deal 15 damage" not "strong units"
- **Use WIP flags** - Mark uncertain sections `<WIP>`, `<NEEDS SPEC>`, etc.
- **Check truth sources** - Reference what exists ✅ vs what's needed ❌
- **Ask questions** - Use `<CLARIFICATION NEEDED:>` for design decisions
- **Link concepts** - How does this interact with other systems?

### DON'T ❌
- **Assume systems exist** - Check truth sources first!
- **State specifics as facts** - Use `<FOR REVIEW>` if uncertain
- **Design in isolation** - Consider impact on other systems
- **Over-commit** - It's okay to have multiple options marked `<WIP>`
- **Ignore existing code** - Check what's implemented before designing

---

## 📞 For AI Agents

When adding concepts:
1. **Read related concepts first** to avoid duplication
2. **Use templates** for consistent structure
3. **Tag appropriately** with status
4. **Link bidirectionally** (update related docs)
5. **Update dashboard** in this README
6. **Cross-reference** truth sources where applicable

When implementing concepts:
1. **Check concept status** (only implement Approved)
2. **Create truth source** contract first
3. **Link truth source** back to concept doc
4. **Update concept status** to "In Development" → "Implemented"
5. **Document deviations** if implementation differs from concept

---

## 🚀 Quick Start: Your First Concept

```bash
# 1. Navigate to concepts
cd Docs/Concepts

# 2. Choose category (e.g., Miracles)
cd Miracles

# 3. Create file
# Name format: PascalCase_With_Underscores.md
touch Heal_Miracle.md

# 4. Add header
echo "# Heal Miracle" > Heal_Miracle.md
echo "" >> Heal_Miracle.md
echo "**Status:** Draft" >> Heal_Miracle.md
echo "**Category:** Miracle - Support" >> Heal_Miracle.md
echo "**Created:** $(date +%Y-%m-%d)" >> Heal_Miracle.md

# 5. Edit and fill in details
# 6. Commit when ready for review
```

---

**Last Updated:** 2025-12-17
**Maintainer:** Godgame Development Team
**Total Concepts:** 21 (2 new system specs added)

---

**Remember:** This repository is for **dreaming big**. Truth sources are for **building real**. Concepts can be wild, ambitious, and experimental. Implementation will ground them in reality.

**⚠️ CURRENT PHASE:** Pure conceptualization - fleshing out game ideas, NOT implementing code yet. Feel free to explore wild ideas, ask big questions, and iterate on design without worrying about technical constraints.

