using System.Collections.Generic;

namespace DagoniteEmpire.Pages.Barony.Components
{
    /// <summary>How the body of a help section is rendered.</summary>
    public enum BaronyHelpBlockKind
    {
        Bullet,
        Paragraph,
    }

    /// <summary>A titled block of help content (e.g. "What you can do").</summary>
    public sealed record BaronyHelpSection(
        string Heading,
        IReadOnlyList<string> Items,
        BaronyHelpBlockKind Kind = BaronyHelpBlockKind.Bullet);

    /// <summary>
    /// Help content for a single Barony card. <see cref="Short"/> is the quick summary
    /// always shown; <see cref="PlayerSections"/> is the detailed, player-facing content.
    /// <see cref="GmSections"/> is authored ahead of time but not rendered yet (player-only for now).
    /// </summary>
    public sealed record BaronyHelpEntry(
        string Key,
        string Title,
        string Short,
        IReadOnlyList<BaronyHelpSection> PlayerSections,
        IReadOnlyList<BaronyHelpSection>? GmSections = null);

    /// <summary>
    /// Central catalog of Barony card help, keyed by the tab key used in <c>BaronyCardTabs</c>.
    /// Pilot coverage: resources, budget, army. Remaining cards are added incrementally.
    /// </summary>
    public static class BaronyHelpCatalog
    {
        private static readonly Dictionary<string, BaronyHelpEntry> ByKey =
            new(System.StringComparer.OrdinalIgnoreCase)
            {
                ["resources"] = new BaronyHelpEntry(
                    Key: "resources",
                    Title: "Resources",
                    Short: "Track your barony's stockpiles, the income you expect this turn, and the running " +
                           "balance that feeds them. Cumulative stores such as food and gold carry over between turns.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Current Resource Stocks — what your granaries and treasury hold right now, mirrored in the top resource bar.",
                            "Expected Income This Turn — the Domain Panel total that is added to stocks when the turn is resolved. Gold already has the liege tribute deducted.",
                            "Resource Balance — a ledger of every source that makes up your stocks; the Σ row must match your current stocks.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Review stockpiles and plan spending on projects, army upkeep and buildings before the turn resolves.",
                            "Read the expected income to see whether you will end the turn in surplus or deficit.",
                            "Audience grants appear here as soon as they are awarded, so you can factor them in immediately.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Adding, editing or removing balance sources and resolving the turn are Game Master actions.",
                            "Tell the Game Master what you intend to build, trade or store; they apply the changes and resolve the turn so income lands in your stocks.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Use Add Source / edit / delete on the Resource Balance to grant or remove resources mid-turn.",
                            "Resolve Turn clears the ledger to opening stock + Domain Panel income + completed project grants.",
                        }),
                    }),

                ["budget"] = new BaronyHelpEntry(
                    Key: "budget",
                    Title: "Budget",
                    Short: "See the gold flowing through your barony: the treasury and your personal purse, the " +
                           "balance for this turn, and the tribute you owe your liege.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Gold Overview — treasury gold, your baron purse, and the treasury turn balance (Domain Panel gold minus liege tribute).",
                            "Barony Treasury — the income and expense breakdown that produces the turn balance.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Check whether the turn ends positive or negative and adjust your plans accordingly.",
                            "See how much of your gross income goes to your liege as tribute, and what share of vassal village gold you keep.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Tribute rates (liege tribute %, vassal fief %) are set by the Game Master.",
                            "Ask the Game Master to adjust rates or record one-off incomes and expenses; you propose, they apply.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Set liege tribute % and vassal fief % and press Save rates.",
                            "Record previous-turn income and manage baron purse sources.",
                        }),
                    }),

                ["army"] = new BaronyHelpEntry(
                    Key: "army",
                    Title: "Army",
                    Short: "Muster and manage your barony's units — recruit through the generator, equip and reinforce " +
                           "them, and watch upkeep in gold, food and defence.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Army totals — unit count, troop count, and per-turn upkeep in gold, food and defence.",
                            "Units — one card per unit showing troops, equipment, skills and status (training or active).",
                            "The unit generator for drafting new units.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Draft new units with the generator and choose their equipment and skills.",
                            "Reinforce, re-equip, rename and inspect the log of each unit.",
                            "Assign a peacetime action (patrol, reconnaissance, training, labour, partial demobilization). Patrol / scout / labour add Domain Skills; partial demobilization halves upkeep; Training XP applies on Resolve Turn.",
                            "Plan upkeep: only active units cost wages, food and defence each turn — training units are free until they graduate.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "During a battle, unit cards are locked and action bonuses / Training XP are suppressed until the Game Master ends the battle.",
                            "The Game Master resolves battles and confirms rewards; coordinate recruitment and deployment with them.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Run and end battles on the Battle Map; roster and HP updates apply on End battle.",
                            "Resolve Turn applies Training XP from unit Training actions.",
                        }),
                    }),

                ["domain"] = new BaronyHelpEntry(
                    Key: "domain",
                    Title: "Domain Panel",
                    Short: "Your barony at a glance. Every management area feeds into this panel — advisors, buildings, " +
                           "relations, terrain, decrees, events and army all roll up into the final PPB totals for the turn.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A meta bar with the current Year, Season, Turn, barony Size, Loyalty, Stability, Economy, Unrest and Conjuncture.",
                            "Collapsible sections mirroring the other cards: Baron and Advisors, City and Buildings, Social Group Relations, Terrain Improvements, Decrees and Technologies, Events, and Army.",
                            "The Barony Summary — the full PPB calculation table combining every source into the totals for the turn.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Read every section's contribution and drill into the Barony Summary to see how each PPB value is built up.",
                            "Toggle decrees on or off for the turn to adjust their effect on your PPB.",
                            "End the turn (or undo it) and mark yourself Ready so the Game Master knows you have finished planning.",
                            "Use the section expand/collapse default switch in the header to show summaries only or full tables.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Editing Unrest and Conjuncture, switching which barony is shown, and resolving the turn are Game Master actions.",
                            "Mark Ready when you are done; the Game Master reviews and resolves the turn to apply all changes.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Edit Unrest and Conjuncture, switch the active barony, and Resolve Turn / clear the ready state.",
                        }),
                    }),

                ["projects"] = new BaronyHelpEntry(
                    Key: "projects",
                    Title: "Projects",
                    Short: "Plan and fund your construction: buildings and terrain improvements start here as projects, " +
                           "and you allocate resources over one or more turns until they are complete.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Project Summary — an overview of all active projects with their costs and expected output.",
                            "Projects — an expandable card per project in progress, with a funding progress bar and details.",
                            "Completed Projects — an archive of finished work.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Add a project to start a new building or improvement.",
                            "Allocate resources to a project card to fund it (gold, food, production, and so on), or clear an allocation to refund it.",
                            "Choose the cost mode (payment method) before you make the first allocation — it locks once funding begins.",
                            "Expand cards to read the description, status, turns remaining, cost breakdown and expected output.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Editing or deleting a project is a Game Master action.",
                            "A project only progresses once fully funded; completion and its output apply on Resolve Turn.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Edit project parameters/costs/outputs, delete projects, and change cost mode after allocation has started.",
                        }),
                    }),

                ["buildings"] = new BaronyHelpEntry(
                    Key: "buildings",
                    Title: "Buildings",
                    Short: "Browse the catalog of buildings and terrain improvements you can construct, with their costs, " +
                           "requirements and PPB effects.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A searchable, sortable table of every building and improvement template, with a count of matching entries.",
                            "Each row shows the description, lordship requirement, type, production and gold cost, and PPB effects (additive and percent).",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Filter the catalog by name and by type (Building or Improvement).",
                            "Sort by any column — name, lordship requirement, type, production cost, gold cost, description or modifiers.",
                            "Propose a building project directly from a city building (the hammer icon) to send it to the Projects card.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Terrain improvements are not started here — you build those by clicking your own tiles on the Terrain map.",
                            "Adding, editing or deleting catalog entries is a Game Master action.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Add new catalog entries; edit or delete custom (non-default) entries.",
                        }),
                    }),

                ["terrain"] = new BaronyHelpEntry(
                    Key: "terrain",
                    Title: "Terrain",
                    Short: "The map of your barony's land. Inspect every tile's terrain, fertility, resources and improvements, " +
                           "and start new improvements on the tiles you hold.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A layers bar and the terrain grid, colour-coded for fertility, terrain type, domains, fiefs, resources and improvements.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Toggle each map layer on or off to focus on fertility, terrain type, domains, fiefs, resources or improvements.",
                            "Hover a tile to inspect its terrain type, fertility, features, resources, improvement and fief/domain.",
                            "Click an empty tile in your own domain to propose a terrain improvement — you can only pick templates compatible with that tile's fertility, resources and type.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Painting fertility, terrain, domains, fiefs, resources and improvements, and resizing the map, are Game Master tools.",
                            "You can only build on your primary domain (not on water); some improvements also need breeding stock from trade.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Brushes for fertility, terrain type, domain, fief, resources and improvements; create/edit/delete domains and fiefs; resize the grid; clear a layer.",
                        }),
                    }),

                ["offices"] = new BaronyHelpEntry(
                    Key: "offices",
                    Title: "Court",
                    Short: "Your court and its officers. See each office, the advisor filling it, the skills they bring to the " +
                           "domain, and the upkeep your court costs each turn.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A section per office (Chancellor, Marshal, and so on) showing the assigned advisor and their contribution.",
                            "The pool of available advisors you can assign, and the total court upkeep in the summary.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "View each advisor's name, description, and main, secondary, domain and combat skills.",
                            "Edit an advisor's profile — name, description — and assign a person from the available pool to an office, or dismiss them.",
                            "See how domain skills and upkeep recalculate as you assign or dismiss advisors.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Adding or removing offices, managing the pool of available court people, and editing custom skill/influence/upkeep sources are Game Master actions.",
                            "Ask the Game Master to introduce new advisors or adjust an office's bonuses.",
                            "The Game Master can attach approved NPC/PC characters as courtiers from Panel MG; their Domain Skills follow the character sheet like the baron's.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Add/remove offices; add/edit/delete available court people; edit custom skill, influence and upkeep sources per office.",
                            "From Panel MG → Attach courtier: pick a barony and an approved character. The person appears in Court with Domain Skills from base + special skills. Detach by removing them from Court.",
                        }),
                    }),

                ["character-card"] = new BaronyHelpEntry(
                    Key: "character-card",
                    Title: "Baron",
                    Short: "Your baron's own card: how their character shapes the barony, their Prestige, Honor and Fear, their " +
                           "time budget, artifacts, reputation tiers, and commander skill tree.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Baron's Influence on the Barony — modifiers coming from the baron's attributes and traits.",
                            "Commander — CX pool and skill tree (same tree as court captains).",
                            "Prestige, Honor & Fear — every source and the totals.",
                            "Baron's Time — the time pool (from Endurance and Willpower) and how it is spent.",
                            "Trophies, Treasures & Artifacts, and Reputation Effects — tiers and the bonuses they grant.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                                                        "Open the commander skill tree and unlock abilities when you have CX.",
"Add, edit or delete custom time actions — expeditions, adventures and other pursuits that cost Baron Time.",
                            "Add, edit or delete time pool modifiers (percent effects from illness, blessings or events).",
                            "Track time spent versus remaining, with warnings when management is under-covered or you overspend.",
                            "Assign trophies, treasures and artifacts to Lord's Seat chambers (adding new items is done by the Game Master).",
                            "Review your Prestige/Honor/Fear sources, artifact placements and reputation tiers with their barony and character bonuses.",
                        }),
                        new BaronyHelpSection("Commander CX", new[]
                        {
                            "Baron and linked character courtiers: CX floor = (permanent Inspire + Strategy and tactics) × 2.",
                            "Simplified court sheets: CX floor = (Command + Strategy/tactics) × 4.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Adding or editing baron influence sources, Prestige/Honor/Fear sources, and artifacts is done by the Game Master.",
                            "Reputation tiers unlock character traits and barony effects; the Game Master confirms adventure and artifact rewards.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Add/edit/delete baron influence sources, Prestige/Honor/Fear sources, and artifacts (with optional domain PPB).",
                            "The baron can assign artifact locations to seat chambers; domain PPB applies only while an item is placed.",
                        }),
                    }),

                ["lords-seat"] = new BaronyHelpEntry(
                    Key: "lords-seat",
                    Title: "Lord's Seat",
                    Short: "Your residence, floor by floor. Assign a purpose to each chamber to shape the Prestige, Honor and Fear " +
                           "your seat radiates and where your artifacts are displayed.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A visual grid of chambers across each floor (Ground, Upper, Tower, Dungeon), plus a table view.",
                            "The seat's Prestige/Honor/Fear summary and each chamber's totals (with its size multiplier).",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Assign or change a chamber's purpose (guest hall, treasury, shrine, and so on) from the available templates.",
                            "Switch between floors and inspect each chamber via tooltip — size, occupant, purpose and prestige multiplier.",
                            "See each chamber's additive and percent modifiers and how they add to the seat's PPB (including items displayed there).",
                            "Assign existing trophies, treasures and artifacts to chambers — their domain PPB applies only while displayed.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Adding or resizing chambers, editing the grid or floors, painting decoration, and managing purpose templates are Game Master tools.",
                            "You can assign a purpose to any chamber that is not marked as a ruin.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Add/remove/edit chambers and floors, resize the grid, paint decoration, mark ruins, and manage seat purpose templates.",
                        }),
                    }),

                ["letters"] = new BaronyHelpEntry(
                    Key: "letters",
                    Title: "Letters",
                    Short: "Your correspondence. Read incoming letters and write replies, or open new threads to reach out to " +
                           "NPCs across the realm.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A thread list grouped into folders by turn, with unread indicators.",
                            "A letter viewer and composer showing each exchange with its sender, in-world date and turn number.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Start a new outbound thread to begin a correspondence.",
                            "Compose and send letters with a rich-text editor (headings, bold, italic, underline, strike, colours, lists, alignment and links).",
                            "Drafts auto-save as you write; you can edit your last sent letter while you await a reply, and delete your own drafts.",
                            "Rename a thread's title and expand or collapse folders to organise your reading.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "The Game Master writes the incoming letters and plays your correspondents.",
                            "While you are waiting for a reply you cannot send again in that thread — start a new thread if you need to.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Create inbound threads, write inbound letters, rename or delete any thread, and edit or delete any draft.",
                        }),
                    }),

                ["audiences"] = new BaronyHelpEntry(
                    Key: "audiences",
                    Title: "Audiences",
                    Short: "Formal petitions and council sessions. Hear those who come before you, respond as the Baron, and track " +
                           "what each audience grants your barony.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A summary of the grants (PPB resources and Prestige/Honor/Fear) flowing from active audiences this turn.",
                            "Council topics (current and archived), the active and deferred Audiences, and an Archive of resolved ones.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Expand an audience to read its full exchange history, the resource/PHP deltas each exchange added, and the Game Master's summary.",
                            "Speak in an audience: compose a message and choose to answer as the Baron or to pose a Question to the Game Master.",
                            "Dismiss an audience to close it when the matter is settled.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Creating audiences, speaking as advisors or NPCs, granting resources or project outcomes, and resolving or deferring audiences are Game Master actions.",
                            "Use 'Question to GM' to ask something out of character; the Game Master answers and drives the petitioners.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Create audiences; speak as any advisor, the GM, or a named NPC; add resources/project outcomes; resolve, defer or dismiss audiences.",
                        }),
                    }),

                ["relations"] = new BaronyHelpEntry(
                    Key: "relations",
                    Title: "Relations",
                    Short: "Your directory of contacts and NPCs, grouped by category, each with an attitude toward you built from " +
                           "underlying modifiers.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A section per relation category, each a table of contacts with name, title, age, description, troop count and attitude.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Read each contact's details and hover the attitude to see the breakdown of every modifier affecting it.",
                            "Add or edit character marks (colour/icon badges) to tag contacts for your own reference.",
                            "Write and edit private notes on any contact.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Adding, editing or deleting a contact and adjusting the attitude modifiers are Game Master actions.",
                            "Marks and notes are yours to manage; the underlying attitude is set by the Game Master.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Add/edit/delete relations and edit the attitude modifiers behind each contact's score.",
                        }),
                    }),

                ["known-lords"] = new BaronyHelpEntry(
                    Key: "known-lords",
                    Title: "Lords",
                    Short: "A shared directory of the Eastern March nobility — a reference for trade and diplomacy that you can " +
                           "annotate for your own barony.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A roster of lords with their house, title, holdings, wealth rating, description and the trade goods they produce.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Search lords by name, house, title, holdings, description, goods or notes.",
                            "Filter by your character marks — by icon type and by colour.",
                            "Add or edit character marks to flag lords, and write private notes visible only to your barony.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "The lords roster itself is shared and read-only; your marks and notes are private to your barony.",
                        }),
                    }),

                ["battle-map"] = new BaronyHelpEntry(
                    Key: "battle-map",
                    Title: "Battle",
                    Short: "The tactical battlefield. Deploy your units, plan their moves and facing, assign attacks, and fight " +
                           "through Setup, Movement, Attack Planning and Combat phases.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "The battle grid with terrain, your forces and the enemy forces panels, the initiative order, the battle log/chat, and the phase actions with hints.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Setup: deploy your units into the green deployment zone and check their HP and stats.",
                            "Movement: plan paths by clicking waypoints, rotate facing, set up a charge, then Finish move (or Undo); a range preview shows how far you can go.",
                            "Attack Planning: click an adjacent enemy to assign it as a target, use Auto-assign attacks, then press I'm ready.",
                            "Use the Full Defense stance, trigger available commander abilities, and hover enemies to read their stats.",
                            "Follow the initiative order, threat/engagement badges, and chat in the battle log.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "The map stays hidden until the Game Master reveals it; the Game Master places enemies and starts/advances/ends the battle.",
                            "Fog of war hides enemy planned paths until they finish moving; all movement resolves at once when everyone has ordered.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Map edit (paint cells, deploy zones, labels), show/hide the map, add/edit enemy tokens, and drive the phases: Begin battle, Resolve movement, Begin combat, End round, End battle.",
                        }),
                    }),

                ["march-map"] = new BaronyHelpEntry(
                    Key: "march-map",
                    Title: "March map",
                    Short: "The map of the realm — capitals, cities, villages, roads and rivers. Chart trade routes from your seat " +
                           "and turn them into treaties.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A legend and the map canvas with your seat highlighted, plus overlays for each active trade route.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Toggle Show goods to see what cities produce, and view the trade-route overlays.",
                            "Design a route from your seat to a city — auto-computed shortest path or manual step-by-step — and review its toll, customs and route economy.",
                            "Turn a designed route into a new trade treaty, or edit and delete existing treaties.",
                            "Hover lord nodes to see their title, holdings and wealth.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Editing the map layout (moving nodes, drawing links, adding places) and blocking lords are Game Master tools.",
                            "You need a seat on the map to design routes; blocking a lord removes treaties that pass through them.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Edit map (move/connect/edit links, add places, set map image), reset layout, and block/unblock lords.",
                        }),
                    }),

                ["trade-goods"] = new BaronyHelpEntry(
                    Key: "trade-goods",
                    Title: "Trade",
                    Short: "Your trade goods and treaties. See which goods you have access to, the bonuses they bring, and the " +
                           "terms of every route you trade along.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "Counts of available, produced and trade-received goods, and a PPB breakdown (additive and percent) of every bonus source.",
                            "The trade treaties list, the luxury goods access tier, and a table of every good with its source, description, effects, unlocks and requirements.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Review each good's source (produced, traded or override), its PPB bonus, what it unlocks and what building or requirement it needs.",
                            "Inspect treaties: the route and its economy bonus, customs and sweeteners per turn, and the goods each side grants and receives.",
                            "Spot 'Trade only' imports that you cannot re-export, and read your luxury access tier and what it grants.",
                            "Create a new treaty (via the March map), or edit and delete existing treaties.",
                        }),
                        new BaronyHelpSection("Working with the Game Master", new[]
                        {
                            "Setting the luxury goods access tier and forcing a good available via override are Game Master actions.",
                        }),
                    },
                    GmSections: new[]
                    {
                        new BaronyHelpSection("Game Master controls", new[]
                        {
                            "Set the luxury goods access tier and toggle per-good MG overrides.",
                        }),
                    }),

                ["notes"] = new BaronyHelpEntry(
                    Key: "notes",
                    Title: "Notes",
                    Short: "Your private planning space — plans, ideas and reminders that only you can see. The baron player and the Game Master each keep their own separate notes.",
                    PlayerSections: new[]
                    {
                        new BaronyHelpSection("What you'll find here", new[]
                        {
                            "A Journal for longer free-form text, plus Sticky notes and turn Reminders.",
                        }),
                        new BaronyHelpSection("What you can do", new[]
                        {
                            "Write rich-text notes in the Journal; it auto-saves and keeps a local backup so nothing is lost if you leave the page suddenly.",
                        }),
                        new BaronyHelpSection("Who can see this", new[]
                        {
                            "Nothing here is shared — the baron player and the Game Master each have their own private notes and cannot see the other's.",
                        }),
                    }),
            };

        public static bool Has(string? key) =>
            !string.IsNullOrWhiteSpace(key) && ByKey.ContainsKey(key);

        public static bool TryGet(string? key, out BaronyHelpEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(key) && ByKey.TryGetValue(key, out var found))
            {
                entry = found;
                return true;
            }

            entry = default!;
            return false;
        }
    }
}
