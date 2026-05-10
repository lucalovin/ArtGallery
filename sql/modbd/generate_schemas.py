import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
import matplotlib.patheffects as pe

# ── helpers ──────────────────────────────────────────────────────────────────

def draw_entity(ax, x, y, w, h, title, attrs, color, title_color="white", dashed=False):
    """Draw an ER entity box with title bar and attribute list."""
    lw = 1.5
    ls = '--' if dashed else '-'
    # shadow
    shadow = FancyBboxPatch((x+0.04, y-0.04), w, h,
                            boxstyle="round,pad=0.02", linewidth=0,
                            facecolor="#00000022", zorder=1)
    ax.add_patch(shadow)
    # body
    body = FancyBboxPatch((x, y), w, h,
                          boxstyle="round,pad=0.02", linewidth=lw,
                          edgecolor="#333333", facecolor="white",
                          linestyle=ls, zorder=2)
    ax.add_patch(body)
    # title bar
    title_h = 0.38
    title_bar = FancyBboxPatch((x, y+h-title_h), w, title_h,
                               boxstyle="round,pad=0.02", linewidth=0,
                               facecolor=color, zorder=3)
    ax.add_patch(title_bar)
    ax.text(x+w/2, y+h-title_h/2, title,
            ha='center', va='center', fontsize=7.5,
            fontweight='bold', color=title_color, zorder=4,
            wrap=True)
    # attributes
    line_h = (h - title_h) / max(len(attrs), 1)
    for i, attr in enumerate(attrs):
        ty = y + h - title_h - (i+0.55)*line_h
        ax.text(x+0.1, ty, attr, ha='left', va='center',
                fontsize=6.2, color="#222222", zorder=4)


def draw_rel(ax, x1, y1, x2, y2, label="", color="#555555"):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="-", color=color,
                                lw=1.4, connectionstyle="arc3,rad=0.0"),
                zorder=0)
    if label:
        mx, my = (x1+x2)/2, (y1+y2)/2
        ax.text(mx, my, label, ha='center', va='center',
                fontsize=5.5, color=color,
                bbox=dict(facecolor='white', edgecolor='none', pad=1))


def legend_patch(color, label):
    return mpatches.Patch(facecolor=color, edgecolor="#333", label=label, linewidth=0.8)


# ── SCHEMA 1 : ORCLPDB (BD_AM) ───────────────────────────────────────────────

def build_am():
    fig, ax = plt.subplots(figsize=(18, 13))
    ax.set_xlim(0, 18)
    ax.set_ylim(0, 13)
    ax.set_aspect('equal')
    ax.axis('off')

    fig.patch.set_facecolor("#F7F8FA")
    ax.set_facecolor("#F7F8FA")

    C_REP   = "#2980B9"   # replicated  - blue
    C_HORIZ = "#27AE60"   # horizontal  - green
    C_VERT  = "#E67E22"   # vertical    - orange
    C_GLOB  = "#8E44AD"   # global      - purple
    C_JUNC  = "#7F8C8D"   # junction    - grey

    W, H = 3.0, 1.9

    # -- ARTGALLERY_AM entities --

    # ARTIST_AM  (replicated)
    ax.text(1.5+W/2, 12.55, "ARTGALLERY_AM", ha='center', va='bottom',
            fontsize=9, fontweight='bold', color="#2C3E50")
    draw_entity(ax, 0.3, 9.8, W, H, "ARTIST_AM",
                ["artist_id  PK", "name", "nationality",
                 "birth_year", "death_year"], C_REP)

    # COLLECTION_AM (replicated)
    draw_entity(ax, 4.2, 9.8, W, H, "COLLECTION_AM",
                ["collection_id  PK", "name", "description",
                 "created_date"], C_REP)

    # EXHIBITOR_AM (horizontal fragment)
    draw_entity(ax, 0.3, 6.8, W, H+0.3, "EXHIBITOR_AM",
                ["exhibitor_id  PK", "name", "address",
                 "city  [= 'New York']", "contact_info"], C_HORIZ)

    # EXHIBITION_AM (horizontal)
    draw_entity(ax, 4.2, 6.8, W, H+0.3, "EXHIBITION_AM",
                ["exhibition_id  PK", "title", "start_date",
                 "end_date", "exhibitor_id  FK"], C_HORIZ)

    # ARTWORK_DETAILS (vertical fragment)
    draw_entity(ax, 8.1, 6.8, W, H+0.3, "ARTWORK_DETAILS",
                ["artwork_id  PK", "location_id", "estimated_value",
                 "(fragment vertical)"], C_VERT)

    # ARTWORK_EXHIBITION_AM (junction)
    draw_entity(ax, 6.0, 4.0, W, H-0.1, "ARTWORK_EXHIBITION_AM",
                ["artwork_id  FK", "exhibition_id  FK",
                 "position_in_gallery", "featured_status"], C_JUNC, dashed=True)

    # LOAN_AM
    draw_entity(ax, 0.3, 3.8, W, H+0.1, "LOAN_AM",
                ["loan_id  PK", "artwork_id  FK",
                 "exhibitor_id  FK", "start_date",
                 "end_date", "conditions"], C_HORIZ)

    # GALLERY_REVIEW_AM
    draw_entity(ax, 4.2, 3.8, W, H+0.1, "GALLERY_REVIEW_AM",
                ["review_id  PK", "visitor_id",
                 "artwork_id  FK", "exhibition_id  FK",
                 "rating (1-5)", "review_date"], C_HORIZ)

    # -- ARTGALLERY_GLOBAL entities --
    gx = 11.5
    ax.text(gx+W/2+0.3, 12.55, "ARTGALLERY_GLOBAL", ha='center', va='bottom',
            fontsize=9, fontweight='bold', color="#2C3E50")

    draw_entity(ax, gx, 10.5, W, H, "LOCATION",
                ["location_id  PK", "name", "gallery_room",
                 "type", "capacity"], C_GLOB)

    draw_entity(ax, gx+3.8, 10.5, W, H, "VISITOR",
                ["visitor_id  PK", "name", "email",
                 "phone", "membership_type", "join_date"], C_GLOB)

    draw_entity(ax, gx, 7.5, W, H, "STAFF",
                ["staff_id  PK", "name", "role",
                 "hire_date", "certification_level"], C_GLOB)

    draw_entity(ax, gx+3.8, 7.5, W, H, "INSURANCE_POLICY",
                ["policy_id  PK", "provider",
                 "start_date", "end_date",
                 "total_coverage_amount"], C_GLOB)

    draw_entity(ax, gx, 4.6, W, H, "INSURANCE",
                ["insurance_id  PK", "artwork_id",
                 "policy_id  FK", "insured_amount"], C_GLOB)

    draw_entity(ax, gx+3.8, 4.6, W, H, "RESTORATION",
                ["restoration_id  PK", "artwork_id",
                 "staff_id  FK", "start_date",
                 "end_date", "description"], C_GLOB)

    draw_entity(ax, gx+1.9, 1.8, W, H, "ACQUISITION",
                ["acquisition_id  PK", "artwork_id  UNIQUE",
                 "acquisition_date", "acquisition_type",
                 "price", "staff_id  FK"], C_GLOB)

    # -- relationships (AM) --
    # EXHIBITOR_AM -> EXHIBITION_AM
    draw_rel(ax, 3.3, 7.6, 4.2, 7.6, "organizes")
    # EXHIBITOR_AM -> LOAN_AM
    draw_rel(ax, 1.8, 6.8, 1.8, 5.9, "gets")
    # EXHIBITION_AM -> ARTWORK_EXHIBITION_AM
    draw_rel(ax, 5.7, 6.8, 7.0, 5.9, "includes")
    # ARTWORK_DETAILS -> ARTWORK_EXHIBITION_AM
    draw_rel(ax, 8.1, 7.6, 9.0, 5.9, "")
    # EXHIBITION_AM -> GALLERY_REVIEW_AM
    draw_rel(ax, 5.7, 6.8, 5.7, 5.9, "has")
    # ARTWORK_DETAILS -> LOAN_AM
    draw_rel(ax, 8.1, 7.5, 3.3, 5.3, "")

    # GLOBAL relationships
    draw_rel(ax, gx+1.5, 7.5, gx+1.5, 6.6, "handles")
    draw_rel(ax, gx+5.3, 7.5, gx+5.3, 6.6, "covers")
    draw_rel(ax, gx+1.5, 4.6, gx+2.9, 3.8, "")
    draw_rel(ax, gx+5.3, 4.6, gx+3.9, 3.8, "")

    # -- legend --
    legend_items = [
        legend_patch(C_REP,   "Relatie replicata"),
        legend_patch(C_HORIZ, "Fragment orizontal"),
        legend_patch(C_VERT,  "Fragment vertical"),
        legend_patch(C_GLOB,  "Relatie globala (nedistribuita)"),
        legend_patch(C_JUNC,  "Tabel de legatura"),
    ]
    ax.legend(handles=legend_items, loc='lower left',
              fontsize=7.5, framealpha=0.9, ncol=2)

    ax.set_title("Schema Conceptuala — ORCLPDB (BD_AM, Statia 1 — Americas)\n"
                 "Scheme: ARTGALLERY_AM  +  ARTGALLERY_GLOBAL",
                 fontsize=13, fontweight='bold', color="#2C3E50", pad=10)

    plt.tight_layout()
    out = r"d:\FMI\Master\Anul II\Sem II\MODBD\PROIECT MODB\ArtGallery\sql\modbd\schema_BD_AM.png"
    plt.savefig(out, dpi=180, bbox_inches='tight', facecolor="#F7F8FA")
    plt.close()
    print(f"Saved: {out}")


# ── SCHEMA 2 : ORCLPDB2 (BD_EU) ──────────────────────────────────────────────

def build_eu():
    fig, ax = plt.subplots(figsize=(14, 10))
    ax.set_xlim(0, 14)
    ax.set_ylim(0, 10)
    ax.set_aspect('equal')
    ax.axis('off')

    fig.patch.set_facecolor("#F7F8FA")
    ax.set_facecolor("#F7F8FA")

    C_REP   = "#2980B9"
    C_HORIZ = "#27AE60"
    C_VERT  = "#E67E22"
    C_JUNC  = "#7F8C8D"

    W, H = 3.0, 1.9

    # ARTIST_EU
    draw_entity(ax, 0.3, 7.5, W, H, "ARTIST_EU",
                ["artist_id  PK", "name", "nationality",
                 "birth_year", "death_year"], C_REP)

    # COLLECTION_EU
    draw_entity(ax, 4.2, 7.5, W, H, "COLLECTION_EU",
                ["collection_id  PK", "name", "description",
                 "created_date"], C_REP)

    # EXHIBITOR_EU
    draw_entity(ax, 0.3, 4.6, W, H+0.3, "EXHIBITOR_EU",
                ["exhibitor_id  PK", "name", "address",
                 "city  [Paris/London/Madrid]", "contact_info"], C_HORIZ)

    # EXHIBITION_EU
    draw_entity(ax, 4.2, 4.6, W, H+0.3, "EXHIBITION_EU",
                ["exhibition_id  PK", "title", "start_date",
                 "end_date", "exhibitor_id  FK"], C_HORIZ)

    # ARTWORK_CORE
    draw_entity(ax, 8.1, 4.6, W, H+0.3, "ARTWORK_CORE",
                ["artwork_id  PK", "title", "artist_id",
                 "year_created", "medium",
                 "collection_id  (fragment vertical)"], C_VERT)

    # ARTWORK_EXHIBITION_EU
    draw_entity(ax, 6.0, 2.0, W, H-0.1, "ARTWORK_EXHIBITION_EU",
                ["artwork_id  FK", "exhibition_id  FK",
                 "position_in_gallery", "featured_status"], C_JUNC, dashed=True)

    # LOAN_EU
    draw_entity(ax, 0.3, 1.8, W, H+0.1, "LOAN_EU",
                ["loan_id  PK", "artwork_id  FK",
                 "exhibitor_id  FK", "start_date",
                 "end_date", "conditions"], C_HORIZ)

    # GALLERY_REVIEW_EU
    draw_entity(ax, 4.2, 1.8, W, H+0.1, "GALLERY_REVIEW_EU",
                ["review_id  PK", "visitor_id",
                 "artwork_id  FK", "exhibition_id  FK",
                 "rating (1-5)", "review_date"], C_HORIZ)

    # relationships
    draw_rel(ax, 3.3, 5.4, 4.2, 5.4, "organizes")
    draw_rel(ax, 1.8, 4.6, 1.8, 3.9, "gets")
    draw_rel(ax, 5.7, 4.6, 7.0, 3.9, "includes")
    draw_rel(ax, 8.1, 5.4, 9.0, 3.9, "")
    draw_rel(ax, 5.7, 4.6, 5.7, 3.9, "has")
    draw_rel(ax, 8.1, 5.3, 3.3, 3.2, "")

    legend_items = [
        legend_patch(C_REP,   "Relatie replicata"),
        legend_patch(C_HORIZ, "Fragment orizontal"),
        legend_patch(C_VERT,  "Fragment vertical"),
        legend_patch(C_JUNC,  "Tabel de legatura"),
    ]
    ax.legend(handles=legend_items, loc='lower right',
              fontsize=7.5, framealpha=0.9)

    ax.set_title("Schema Conceptuala — ORCLPDB2 (BD_EU, Statia 2 — Europe)\n"
                 "Schema: ARTGALLERY_EU",
                 fontsize=13, fontweight='bold', color="#2C3E50", pad=10)

    plt.tight_layout()
    out = r"d:\FMI\Master\Anul II\Sem II\MODBD\PROIECT MODB\ArtGallery\sql\modbd\schema_BD_EU.png"
    plt.savefig(out, dpi=180, bbox_inches='tight', facecolor="#F7F8FA")
    plt.close()
    print(f"Saved: {out}")


build_am()
build_eu()
print("Done.")
