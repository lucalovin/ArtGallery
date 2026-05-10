from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    HRFlowable, KeepTogether
)
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_JUSTIFY
from reportlab.platypus import PageBreak

OUT = r"d:\FMI\Master\Anul II\Sem II\MODBD\PROIECT MODB\ArtGallery\sql\modbd\Modul2_Cerinta8_Constrangeri.pdf"

# ── colours ──────────────────────────────────────────────────────────────────
BLUE      = colors.HexColor("#2980B9")
GREEN     = colors.HexColor("#27AE60")
ORANGE    = colors.HexColor("#E67E22")
PURPLE    = colors.HexColor("#8E44AD")
GREY      = colors.HexColor("#7F8C8D")
DARK      = colors.HexColor("#2C3E50")
LIGHT_BG  = colors.HexColor("#EBF5FB")
LIGHT_GR  = colors.HexColor("#EAFAF1")
LIGHT_OR  = colors.HexColor("#FEF9E7")
LIGHT_PU  = colors.HexColor("#F5EEF8")
WHITE     = colors.white
RED_LIGHT = colors.HexColor("#FDEDEC")

# ── styles ────────────────────────────────────────────────────────────────────
styles = getSampleStyleSheet()

def S(name, **kw):
    base = styles[name] if name in styles else styles["Normal"]
    return ParagraphStyle(name + str(id(kw)), parent=base, **kw)

H1  = S("Heading1", fontSize=18, textColor=DARK, spaceAfter=6, spaceBefore=4,
        fontName="Helvetica-Bold", alignment=TA_CENTER)
H2  = S("Heading2", fontSize=13, textColor=BLUE, spaceAfter=4, spaceBefore=10,
        fontName="Helvetica-Bold")
H3  = S("Heading3", fontSize=11, textColor=DARK, spaceAfter=3, spaceBefore=6,
        fontName="Helvetica-Bold")
H4  = S("Heading4", fontSize=10, textColor=GREY, spaceAfter=2, spaceBefore=4,
        fontName="Helvetica-BoldOblique")
NR  = S("Normal", fontSize=9, leading=14, spaceAfter=2, alignment=TA_JUSTIFY)
NR_MONO = S("Normal", fontSize=8, leading=12, fontName="Courier", textColor=DARK)
SUB = S("Normal", fontSize=8, leading=12, textColor=GREY, spaceAfter=1)
TITLE_SUB = S("Normal", fontSize=11, textColor=GREY, alignment=TA_CENTER,
              spaceAfter=2)

def hr(color=BLUE, width=1):
    return HRFlowable(width="100%", thickness=width, color=color, spaceAfter=4, spaceBefore=2)

def sp(h=0.2):
    return Spacer(1, h*cm)

def p(text, style=NR):
    return Paragraph(text, style)

def h2(text): return p(text, H2)
def h3(text): return p(text, H3)
def h4(text): return p(text, H4)

def badge(text, bg=BLUE):
    return Paragraph(
        f'<font color="white"><b> {text} </b></font>',
        ParagraphStyle("badge", parent=NR, backColor=bg,
                       borderPadding=(2,4,2,4), fontSize=8,
                       fontName="Helvetica-Bold")
    )

def info_box(text, bg=LIGHT_BG, border=BLUE):
    style = ParagraphStyle("info", parent=NR, backColor=bg,
                           borderColor=border, borderWidth=1,
                           borderPadding=(6,8,6,8), fontSize=9,
                           leading=14)
    return Paragraph(text, style)

def table_simple(data, col_widths, header_bg=BLUE, row_bg=LIGHT_BG, alt_bg=WHITE):
    col_styles = [
        ('BACKGROUND', (0,0), (-1,0), header_bg),
        ('TEXTCOLOR',  (0,0), (-1,0), WHITE),
        ('FONTNAME',   (0,0), (-1,0), 'Helvetica-Bold'),
        ('FONTSIZE',   (0,0), (-1,-1), 8),
        ('LEADING',    (0,0), (-1,-1), 11),
        ('GRID',       (0,0), (-1,-1), 0.4, colors.HexColor("#BDC3C7")),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [row_bg, WHITE]),
        ('VALIGN',     (0,0), (-1,-1), 'MIDDLE'),
        ('LEFTPADDING',(0,0), (-1,-1), 5),
        ('RIGHTPADDING',(0,0),(-1,-1), 5),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING',(0,0),(-1,-1), 3),
    ]
    t = Table(data, colWidths=col_widths, repeatRows=1)
    t.setStyle(TableStyle(col_styles))
    return t

# ─────────────────────────────────────────────────────────────────────────────
# CONTENT
# ─────────────────────────────────────────────────────────────────────────────

story = []

# ── COVER ────────────────────────────────────────────────────────────────────
story += [
    sp(2),
    p("Facultatea de Matematica si Informatica", TITLE_SUB),
    p("Master Informatica — Modulul MODBD, 2025-2026", TITLE_SUB),
    sp(0.5),
    hr(BLUE, 2),
    p("Proiect: Galeria de Arta (ArtGallery)<br/>"
      "Baze de Date Distribuite — Oracle 19c", H1),
    hr(BLUE, 2),
    sp(0.8),
    p("Cerinta 8 (Modul Analiza) — 2 puncte", H1),
    p("Lista tuturor constrangerilor ce trebuie indeplinite de model", TITLE_SUB),
    sp(1.5),
    info_box(
        "<b>Arhitectura sistemului distribuit:</b><br/>"
        "&#8226; <b>ORCLPDB (BD_AM)</b> — Statia 1 Americas: scheme ARTGALLERY_AM + ARTGALLERY_GLOBAL<br/>"
        "&#8226; <b>ORCLPDB2 (BD_EU)</b> — Statia 2 Europe: schema ARTGALLERY_EU<br/>"
        "&#8226; Fragmentare orizontala: EXHIBITOR, EXHIBITION, LOAN, GALLERY_REVIEW<br/>"
        "&#8226; Fragmentare verticala: ARTWORK (ARTWORK_CORE pe EU + ARTWORK_DETAILS pe AM)<br/>"
        "&#8226; Replicare completa: ARTIST, COLLECTION (identice pe AM si EU)<br/>"
        "&#8226; Relatii nedistribuite: LOCATION, VISITOR, STAFF, INSURANCE, RESTORATION, ACQUISITION"
    ),
    PageBreak(),
]

# ═══════════════════════════════════════════════════════════════════════
# 8a — UNICITATE
# ═══════════════════════════════════════════════════════════════════════
story += [
    h2("8a. Unicitate (0,5 puncte)"),
    hr(BLUE),
    sp(0.2),
]

# 8a.i — UNICITATE LOCALA
story += [
    h3("8a.i — Unicitate locala"),
    p("Fiecare tabel din baza de date distribuita are definita o cheie primara locala "
      "care garanteaza unicitatea randurilor in cadrul acelei statii. "
      "Constrangerile sunt impuse nativ de Oracle prin mecanismul PRIMARY KEY.", NR),
    sp(0.3),
]

data_local = [
    [p("<b>Statie / Schema</b>", NR), p("<b>Tabel</b>", NR),
     p("<b>Coloana(e) unice</b>", NR), p("<b>Tip</b>", NR)],

    [p("BD_AM / ARTGALLERY_AM", NR), p("EXHIBITOR_AM", NR_MONO),
     p("exhibitor_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("EXHIBITION_AM", NR_MONO),
     p("exhibition_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("ARTWORK_DETAILS", NR_MONO),
     p("artwork_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("ARTWORK_EXHIBITION_AM", NR_MONO),
     p("(artwork_id, exhibition_id)", NR_MONO), p("PRIMARY KEY compusa", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("LOAN_AM", NR_MONO),
     p("loan_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("GALLERY_REVIEW_AM", NR_MONO),
     p("review_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("ARTIST_AM", NR_MONO),
     p("artist_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_AM", NR), p("COLLECTION_AM", NR_MONO),
     p("collection_id", NR_MONO), p("PRIMARY KEY", SUB)],

    [p("BD_EU / ARTGALLERY_EU", NR), p("EXHIBITOR_EU", NR_MONO),
     p("exhibitor_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("ARTWORK_CORE", NR_MONO),
     p("artwork_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("EXHIBITION_EU", NR_MONO),
     p("exhibition_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("ARTWORK_EXHIBITION_EU", NR_MONO),
     p("(artwork_id, exhibition_id)", NR_MONO), p("PRIMARY KEY compusa", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("LOAN_EU", NR_MONO),
     p("loan_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("GALLERY_REVIEW_EU", NR_MONO),
     p("review_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("ARTIST_EU", NR_MONO),
     p("artist_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_EU / ARTGALLERY_EU", NR), p("COLLECTION_EU", NR_MONO),
     p("collection_id", NR_MONO), p("PRIMARY KEY", SUB)],

    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("LOCATION", NR_MONO),
     p("location_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("VISITOR", NR_MONO),
     p("visitor_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("STAFF", NR_MONO),
     p("staff_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("INSURANCE_POLICY", NR_MONO),
     p("policy_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("INSURANCE", NR_MONO),
     p("insurance_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("RESTORATION", NR_MONO),
     p("restoration_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("ACQUISITION", NR_MONO),
     p("acquisition_id", NR_MONO), p("PRIMARY KEY", SUB)],
    [p("BD_AM / ARTGALLERY_GLOBAL", NR), p("ACQUISITION", NR_MONO),
     p("artwork_id", NR_MONO), p("UNIQUE (o opera e achizitionata o singura data)", SUB)],
]
story.append(table_simple(data_local, [4.5*cm, 4*cm, 5*cm, 4.5*cm]))
story.append(sp(0.4))

# 8a.ii — UNICITATE GLOBALA ORIZONTALA
story += [
    h3("8a.ii — Unicitate globala pe fragmente orizontale"),
    p("Pentru relatiile fragmentate orizontal (EXHIBITOR, EXHIBITION, LOAN, GALLERY_REVIEW), "
      "cheia primara este locala, dar valoarea cheii trebuie sa fie unica <i>la nivel global</i>, "
      "adica sa nu existe acelasi ID atat pe statia AM cat si pe statia EU. "
      "Oracle nu suporta constrangeri FOREIGN KEY sau UNIQUE cross-database, "
      "deci unicitatea globala este asigurata prin <b>triggere BEFORE INSERT</b> "
      "care interogheaza statia opusa prin DB Link.", NR),
    sp(0.25),
]

data_global_horiz = [
    [p("<b>Relatie logica</b>", NR), p("<b>Fragment AM</b>", NR),
     p("<b>Fragment EU</b>", NR), p("<b>Cheie verificata global</b>", NR),
     p("<b>Mecanism</b>", NR)],

    [p("EXHIBITOR", NR), p("EXHIBITOR_AM", NR_MONO), p("EXHIBITOR_EU", NR_MONO),
     p("exhibitor_id", NR_MONO),
     p("TRG_UNIQ_GLOBAL_EXHIBITOR_AM\nTRG_UNIQ_GLOBAL_EXHIBITOR_EU", NR_MONO)],

    [p("EXHIBITION", NR), p("EXHIBITION_AM", NR_MONO), p("EXHIBITION_EU", NR_MONO),
     p("exhibition_id", NR_MONO),
     p("TRG_UNIQ_GLOBAL_EXHIBITION_AM\nTRG_UNIQ_GLOBAL_EXHIBITION_EU", NR_MONO)],

    [p("LOAN", NR), p("LOAN_AM", NR_MONO), p("LOAN_EU", NR_MONO),
     p("loan_id", NR_MONO),
     p("TRG_UNIQ_GLOBAL_LOAN_AM\nTRG_UNIQ_GLOBAL_LOAN_EU", NR_MONO)],

    [p("GALLERY_REVIEW", NR), p("GALLERY_REVIEW_AM", NR_MONO), p("GALLERY_REVIEW_EU", NR_MONO),
     p("review_id", NR_MONO),
     p("TRG_UNIQ_GLOBAL_REVIEW_AM\nTRG_UNIQ_GLOBAL_REVIEW_EU", NR_MONO)],
]
story.append(table_simple(data_global_horiz, [2.8*cm, 3.5*cm, 3.5*cm, 2.8*cm, 5.4*cm],
                          header_bg=GREEN, row_bg=LIGHT_GR))
story.append(sp(0.25))
story.append(info_box(
    "<b>Exemplu trigger (BEFORE INSERT pe EXHIBITOR_AM):</b><br/>"
    "<font face='Courier' size='8'>IF COUNT(*) &gt; 0 FROM ARTGALLERY_EU.EXHIBITOR_EU@link_eu "
    "WHERE exhibitor_id = :NEW.exhibitor_id THEN<br/>"
    "&nbsp;&nbsp;&nbsp;RAISE_APPLICATION_ERROR(-20010, 'Unicitate globala incalcata');<br/>"
    "END IF;</font>",
    bg=LIGHT_GR, border=GREEN
))
story.append(sp(0.4))

# 8a.iii — UNICITATE GLOBALA FRAGMENTE VERTICALE
story += [
    h3("8a.iii — Unicitate globala in cazul fragmentelor verticale"),
    p("Relatia <b>ARTWORK</b> este fragmentata vertical astfel:", NR),
    p("&nbsp;&nbsp;&#8226; <b>ARTWORK_CORE</b> (BD_EU): artwork_id, title, artist_id, year_created, medium, collection_id", NR),
    p("&nbsp;&nbsp;&#8226; <b>ARTWORK_DETAILS</b> (BD_AM): artwork_id, location_id, estimated_value", NR),
    sp(0.2),
    p("<b>Coloana de jonctiune (cheie de reconstructie):</b> artwork_id — declarata PRIMARY KEY "
      "in ambele fragmente. Aceasta garanteaza ca fiecare opera de arta apare o singura data "
      "in fiecare fragment vertical (relatie 1-la-1 intre fragmente).", NR),
    sp(0.2),
    p("<b>Analiza necesitatii unui UNIQUE cross-fragment:</b>", NR),
    sp(0.1),
]

data_vert = [
    [p("<b>Scenariu</b>", NR), p("<b>Coloane implicate</b>", NR),
     p("<b>Decizie</b>", NR), p("<b>Argumentare (optimizare)</b>", NR)],

    [p("Titlu unic global", NR),
     p("title (pe EU)", NR_MONO),
     p("NU se impune UNIQUE global", NR),
     p("Titluri similare pot exista pentru epoci diferite. "
       "Verificarea ar necesita un trigger cu DB Link la fiecare INSERT pe ARTWORK_CORE, "
       "adaugand latenta de retea fara beneficiu functional.", NR)],

    [p("(title, artist_id) unic global", NR),
     p("title + artist_id\n(ambele pe EU)", NR_MONO),
     p("Se poate impune LOCAL pe EU", NR),
     p("Ambele coloane sunt in ARTWORK_CORE (acelasi fragment), "
       "deci un index UNIQUE local pe EU este suficient si eficient — "
       "nu necesita cross-DB query.", NR)],

    [p("(title, location_id) unic global", NR),
     p("title (EU)\nlocation_id (AM)", NR_MONO),
     p("Se EVITA", NR),
     p("Coloanele sunt pe statii diferite. Impunerea ar necesita un trigger "
       "BEFORE INSERT care face JOIN cross-DB la fiecare operatie. "
       "Costul de retea al verificarii depaseste beneficiul; "
       "business logic-ul nu impune aceasta regula.", NR)],

    [p("artwork_id unic global\n(cheie reconstructie)", NR),
     p("artwork_id\n(PE AMBELE)", NR_MONO),
     p("DA — impus prin PK", NR),
     p("Esential pentru reconstructia relatiei ARTWORK prin JOIN. "
       "Unicitatea locala (PK) in fiecare fragment este suficienta "
       "deoarece datele initiale provin dintr-o singura sursa (BDDALL) "
       "si se insereaza consistent in ambele fragmente simultan.", NR)],
]
story.append(table_simple(data_vert, [3*cm, 3.2*cm, 3.5*cm, 8.3*cm],
                          header_bg=ORANGE, row_bg=LIGHT_OR))
story.append(sp(0.25))
story.append(info_box(
    "<b>Concluzie:</b> In cazul fragmentarii verticale a relatiei ARTWORK, "
    "unicitatea globala este garantata prin cheia primara <i>artwork_id</i> definita local "
    "in fiecare fragment. Nu se identifica combinatii de coloane cross-fragment pentru care "
    "o constrangere UNIQUE globala sa fie justificata functional si eficienta din punct de "
    "vedere al optimizarii (cost retea vs. beneficiu). Orice unicitate la nivel de atribute "
    "descriptive (title, medium) se poate impune local pe fragmentul EU.",
    bg=LIGHT_OR, border=ORANGE
))

story.append(PageBreak())

# ═══════════════════════════════════════════════════════════════════════
# 8b — CHEIE PRIMARA
# ═══════════════════════════════════════════════════════════════════════
story += [
    h2("8b. Cheie primara — nivel local si global (0,5 puncte)"),
    hr(BLUE),
    sp(0.2),
    h3("Chei primare locale"),
    p("Fiecare tabel are o cheie primara definita nativ in Oracle (constraint PRIMARY KEY), "
      "aplicata si indexata automat. Lista completa este prezentata in sectiunea 8a.i.", NR),
    sp(0.3),
    h3("Chei primare la nivel global"),
    p("Intr-un sistem distribuit, cheia primara <i>logica</i> a unei relatii (inainte de fragmentare) "
      "trebuie sa fie unica pe <b>intregul sistem</b>, nu doar local. "
      "Tabelul urmator prezinta situatia fiecarei relatii:", NR),
    sp(0.2),
]

data_pk = [
    [p("<b>Relatie logica</b>", NR), p("<b>Cheie primara logica</b>", NR),
     p("<b>Tip fragmentare</b>", NR), p("<b>Unicitate globala PK</b>", NR),
     p("<b>Mecanism garantare</b>", NR)],

    [p("EXHIBITOR", NR), p("exhibitor_id", NR_MONO), p("Orizontala", NR),
     p("Necesara cross-statie", NR),
     p("Trigger BEFORE INSERT verifica cealalta statie via DB Link", NR)],

    [p("EXHIBITION", NR), p("exhibition_id", NR_MONO), p("Orizontala", NR),
     p("Necesara cross-statie", NR),
     p("Trigger BEFORE INSERT verifica cealalta statie via DB Link", NR)],

    [p("LOAN", NR), p("loan_id", NR_MONO), p("Orizontala", NR),
     p("Necesara cross-statie", NR),
     p("Trigger BEFORE INSERT verifica cealalta statie via DB Link", NR)],

    [p("GALLERY_REVIEW", NR), p("review_id", NR_MONO), p("Orizontala", NR),
     p("Necesara cross-statie", NR),
     p("Trigger BEFORE INSERT verifica cealalta statie via DB Link", NR)],

    [p("ARTWORK", NR), p("artwork_id", NR_MONO), p("Verticala", NR),
     p("Garantata local (PK in fiecare fragment)", NR),
     p("INSERT consistent in ambele fragmente din sursa unica BDDALL", NR)],

    [p("ARTIST", NR), p("artist_id", NR_MONO), p("Replicare completa", NR),
     p("Garantata — identica pe ambele statii", NR),
     p("Trigger de sincronizare propaga INSERT/UPDATE/DELETE bidirectional", NR)],

    [p("COLLECTION", NR), p("collection_id", NR_MONO), p("Replicare completa", NR),
     p("Garantata — identica pe ambele statii", NR),
     p("Trigger de sincronizare propaga INSERT/UPDATE/DELETE bidirectional", NR)],

    [p("LOCATION", NR), p("location_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local (singura instanta)", NR),
     p("PRIMARY KEY Oracle nativ", NR)],

    [p("VISITOR", NR), p("visitor_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],

    [p("STAFF", NR), p("staff_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],

    [p("INSURANCE_POLICY", NR), p("policy_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],

    [p("INSURANCE", NR), p("insurance_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],

    [p("RESTORATION", NR), p("restoration_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],

    [p("ACQUISITION", NR), p("acquisition_id", NR_MONO), p("Nedistribuita (GLOBAL)", NR),
     p("Garantata local", NR), p("PRIMARY KEY Oracle nativ", NR)],
]
story.append(table_simple(data_pk, [2.8*cm, 2.8*cm, 3*cm, 4.4*cm, 5*cm]))

story.append(PageBreak())

# ═══════════════════════════════════════════════════════════════════════
# 8c — CHEIE EXTERNA
# ═══════════════════════════════════════════════════════════════════════
story += [
    h2("8c. Cheie externa — nivel local si cross-database (0,5 puncte)"),
    hr(BLUE),
    sp(0.2),
    h3("Chei externe locale (impuse nativ de Oracle)"),
    p("Urmatoarele referinte sunt definite ca REFERENCES in DDL si sunt "
      "verificate automat de Oracle la INSERT/UPDATE/DELETE:", NR),
    sp(0.2),
]

data_fk_local = [
    [p("<b>Schema</b>", NR), p("<b>Tabel sursa</b>", NR),
     p("<b>Coloana FK</b>", NR), p("<b>Tabel referit</b>", NR),
     p("<b>Coloana PK</b>", NR)],

    [p("ARTGALLERY_AM", NR), p("EXHIBITION_AM", NR_MONO),
     p("exhibitor_id", NR_MONO), p("EXHIBITOR_AM", NR_MONO), p("exhibitor_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("ARTWORK_EXHIBITION_AM", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_DETAILS", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("ARTWORK_EXHIBITION_AM", NR_MONO),
     p("exhibition_id", NR_MONO), p("EXHIBITION_AM", NR_MONO), p("exhibition_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("LOAN_AM", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_DETAILS", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("LOAN_AM", NR_MONO),
     p("exhibitor_id", NR_MONO), p("EXHIBITOR_AM", NR_MONO), p("exhibitor_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("GALLERY_REVIEW_AM", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_DETAILS", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_AM", NR), p("GALLERY_REVIEW_AM", NR_MONO),
     p("exhibition_id", NR_MONO), p("EXHIBITION_AM", NR_MONO), p("exhibition_id", NR_MONO)],

    [p("ARTGALLERY_EU", NR), p("EXHIBITION_EU", NR_MONO),
     p("exhibitor_id", NR_MONO), p("EXHIBITOR_EU", NR_MONO), p("exhibitor_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("ARTWORK_EXHIBITION_EU", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_CORE", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("ARTWORK_EXHIBITION_EU", NR_MONO),
     p("exhibition_id", NR_MONO), p("EXHIBITION_EU", NR_MONO), p("exhibition_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("LOAN_EU", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_CORE", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("LOAN_EU", NR_MONO),
     p("exhibitor_id", NR_MONO), p("EXHIBITOR_EU", NR_MONO), p("exhibitor_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("GALLERY_REVIEW_EU", NR_MONO),
     p("artwork_id", NR_MONO), p("ARTWORK_CORE", NR_MONO), p("artwork_id", NR_MONO)],
    [p("ARTGALLERY_EU", NR), p("GALLERY_REVIEW_EU", NR_MONO),
     p("exhibition_id", NR_MONO), p("EXHIBITION_EU", NR_MONO), p("exhibition_id", NR_MONO)],

    [p("ARTGALLERY_GLOBAL", NR), p("INSURANCE", NR_MONO),
     p("policy_id", NR_MONO), p("INSURANCE_POLICY", NR_MONO), p("policy_id", NR_MONO)],
    [p("ARTGALLERY_GLOBAL", NR), p("RESTORATION", NR_MONO),
     p("staff_id", NR_MONO), p("STAFF", NR_MONO), p("staff_id", NR_MONO)],
    [p("ARTGALLERY_GLOBAL", NR), p("ACQUISITION", NR_MONO),
     p("staff_id", NR_MONO), p("STAFF", NR_MONO), p("staff_id", NR_MONO)],
]
story.append(table_simple(data_fk_local, [3.5*cm, 4*cm, 3.2*cm, 4*cm, 3.3*cm]))
story.append(sp(0.4))

story += [
    h3("Chei externe cross-database (impuse prin triggere BEFORE INSERT)"),
    p("Oracle nu permite FOREIGN KEY intre baze de date diferite. "
      "Urmatoarele referinte traverseaza granita dintre statii si sunt simulate "
      "prin triggere PL/SQL care interogheaza tabelul referit prin DB Link.", NR),
    sp(0.2),
]

data_fk_cross = [
    [p("<b>Trigger</b>", NR), p("<b>Tabel sursa (statie)</b>", NR),
     p("<b>Coloana FK</b>", NR), p("<b>Tabel referit (statie)</b>", NR),
     p("<b>DB Link</b>", NR)],

    [p("TRG_FK_REVIEW_VISITOR_GLOBAL", NR_MONO),
     p("GALLERY_REVIEW_AM (BD_AM)", NR),
     p("visitor_id", NR_MONO),
     p("VISITOR @ ARTGALLERY_GLOBAL (BD_AM)", NR),
     p("link_global", NR_MONO)],

    [p("TRG_FK_REVIEW_EU_VISITOR", NR_MONO),
     p("GALLERY_REVIEW_EU (BD_EU)", NR),
     p("visitor_id", NR_MONO),
     p("VISITOR @ ARTGALLERY_GLOBAL (BD_AM)", NR),
     p("link_global", NR_MONO)],

    [p("TRG_FK_ARTWORK_LOCATION_GLOBAL", NR_MONO),
     p("ARTWORK_DETAILS (BD_AM)", NR),
     p("location_id", NR_MONO),
     p("LOCATION @ ARTGALLERY_GLOBAL (BD_AM)", NR),
     p("link_global", NR_MONO)],

    [p("TRG_FK_INSURANCE_ARTWORK", NR_MONO),
     p("INSURANCE (BD_AM / GLOBAL)", NR),
     p("artwork_id", NR_MONO),
     p("ARTWORK_CORE (BD_EU)\n+ ARTWORK_DETAILS (BD_AM)", NR),
     p("link_eu + local", NR_MONO)],

    [p("TRG_FK_RESTORATION_ARTWORK", NR_MONO),
     p("RESTORATION (BD_AM / GLOBAL)", NR),
     p("artwork_id", NR_MONO),
     p("ARTWORK_CORE (BD_EU)\n+ ARTWORK_DETAILS (BD_AM)", NR),
     p("link_eu + local", NR_MONO)],

    [p("TRG_FK_ACQUISITION_ARTWORK", NR_MONO),
     p("ACQUISITION (BD_AM / GLOBAL)", NR),
     p("artwork_id", NR_MONO),
     p("ARTWORK_CORE (BD_EU)\n+ ARTWORK_DETAILS (BD_AM)", NR),
     p("link_eu + local", NR_MONO)],
]
story.append(table_simple(data_fk_cross,
                          [5*cm, 4*cm, 2.5*cm, 4.5*cm, 2*cm],
                          header_bg=PURPLE, row_bg=LIGHT_PU))
story.append(sp(0.25))
story.append(info_box(
    "<b>Nota:</b> Pentru relatia ARTWORK_CORE.artist_id (referinta catre ARTIST) si "
    "ARTWORK_CORE.collection_id (referinta catre COLLECTION), constrangerile FK nu necesita "
    "DB Link separat deoarece ARTIST si COLLECTION sunt <b>replicate complet</b> pe ambele statii. "
    "Verificarea se face local (pe ARTIST_EU / COLLECTION_EU) pe statia EU.",
    bg=LIGHT_PU, border=PURPLE
))

story.append(PageBreak())

# ═══════════════════════════════════════════════════════════════════════
# 8d — VALIDARE
# ═══════════════════════════════════════════════════════════════════════
story += [
    h2("8d. Validare — nivel local si cross-database (0,5 puncte)"),
    hr(BLUE),
    sp(0.2),
    h3("Constrangeri CHECK locale (impuse nativ de Oracle)"),
    p("Urmatoarele constrangeri sunt definite direct in DDL si sunt verificate "
      "automat de motorul Oracle la orice operatie de modificare a datelor:", NR),
    sp(0.2),
]

data_check = [
    [p("<b>Tabel</b>", NR), p("<b>Statie / Schema</b>", NR),
     p("<b>Expresie CHECK</b>", NR), p("<b>Semantica</b>", NR)],

    [p("EXHIBITION_AM", NR_MONO), p("BD_AM / ARTGALLERY_AM", NR),
     p("end_date >= start_date", NR_MONO),
     p("Expozitia nu poate incheia inainte de a incepe", NR)],

    [p("EXHIBITION_EU", NR_MONO), p("BD_EU / ARTGALLERY_EU", NR),
     p("end_date >= start_date", NR_MONO),
     p("Expozitia nu poate incheia inainte de a incepe", NR)],

    [p("LOAN_AM", NR_MONO), p("BD_AM / ARTGALLERY_AM", NR),
     p("end_date IS NULL\nOR end_date >= start_date", NR_MONO),
     p("Imprumut activ (fara data de sfarsit) sau cu interval valid", NR)],

    [p("LOAN_EU", NR_MONO), p("BD_EU / ARTGALLERY_EU", NR),
     p("end_date IS NULL\nOR end_date >= start_date", NR_MONO),
     p("Imprumut activ (fara data de sfarsit) sau cu interval valid", NR)],

    [p("GALLERY_REVIEW_AM", NR_MONO), p("BD_AM / ARTGALLERY_AM", NR),
     p("rating BETWEEN 1 AND 5", NR_MONO),
     p("Nota recenzie trebuie sa fie intre 1 si 5 (scala Likert)", NR)],

    [p("GALLERY_REVIEW_EU", NR_MONO), p("BD_EU / ARTGALLERY_EU", NR),
     p("rating BETWEEN 1 AND 5", NR_MONO),
     p("Nota recenzie trebuie sa fie intre 1 si 5 (scala Likert)", NR)],

    [p("LOCATION", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("capacity IS NULL\nOR capacity > 0", NR_MONO),
     p("Capacitatea locatiei trebuie sa fie pozitiva (daca este specificata)", NR)],

    [p("INSURANCE_POLICY", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("end_date >= start_date", NR_MONO),
     p("Polita de asigurare trebuie sa aiba un interval de valabilitate valid", NR)],

    [p("INSURANCE_POLICY", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("total_coverage_amount IS NULL\nOR total_coverage_amount >= 0", NR_MONO),
     p("Suma totala acoperita trebuie sa fie nenegativa", NR)],

    [p("INSURANCE", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("insured_amount > 0", NR_MONO),
     p("Suma asigurata trebuie sa fie strict pozitiva", NR)],

    [p("RESTORATION", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("end_date IS NULL\nOR end_date >= start_date", NR_MONO),
     p("Restaurarea activa sau cu interval valid", NR)],

    [p("ACQUISITION", NR_MONO), p("BD_AM / ARTGALLERY_GLOBAL", NR),
     p("price IS NULL\nOR price >= 0", NR_MONO),
     p("Pretul de achizitie trebuie sa fie nenegativ (daca este specificat)", NR)],
]
story.append(table_simple(data_check, [3.8*cm, 4.2*cm, 4.5*cm, 5.5*cm]))
story.append(sp(0.4))

story += [
    h3("Constrangeri de validare cross-database (impuse prin triggere BEFORE INSERT)"),
    p("Urmatoarele reguli de validare implica date din tabelul sursa si un tabel referit "
      "aflat pe o alta statie, deci nu pot fi exprimate ca CHECK constraints Oracle standard. "
      "Sunt implementate ca triggere BEFORE INSERT pe tabelul sursa:", NR),
    sp(0.2),
]

data_val_cross = [
    [p("<b>Trigger</b>", NR), p("<b>Tabel / Statie</b>", NR),
     p("<b>Regula de validare</b>", NR), p("<b>Tabel referit (statie)</b>", NR)],

    [p("TRG_VALIDATE_INSURANCE_AMOUNT", NR_MONO),
     p("INSURANCE\n(BD_AM / GLOBAL)", NR),
     p("insured_amount <= estimated_value * 1.5\n"
       "(suma asigurata nu poate depasi de 1,5 ori valoarea estimata a operei)", NR),
     p("ARTWORK_DETAILS.estimated_value\n(BD_AM) via link_am", NR_MONO)],

    [p("TRG_VALIDATE_RESTORATION_DATE", NR_MONO),
     p("RESTORATION\n(BD_AM / GLOBAL)", NR),
     p("start_date <= SYSDATE\n"
       "(nu se pot introduce restaurari cu data de incepere in viitor)", NR),
     p("SYSDATE — verificare locala\n(nu necesita DB Link)", NR_MONO)],
]
story.append(table_simple(data_val_cross, [5*cm, 3.5*cm, 6*cm, 3.5*cm],
                          header_bg=ORANGE, row_bg=LIGHT_OR))
story.append(sp(0.3))

story.append(info_box(
    "<b>Rezumat mecanism de implementare a constrangerilor cross-database:</b><br/>"
    "Deoarece Oracle Database nu permite constrangeri FOREIGN KEY, CHECK sau UNIQUE "
    "referitoare la tabele din baze de date diferite (accesibile prin DB Link), "
    "toate constrangerile distribuite sunt simulate prin triggere PL/SQL de tip "
    "<b>BEFORE INSERT [OR UPDATE]</b>. "
    "In caz de incalcare a constrangerii, triggerul apeleaza "
    "<font face='Courier'>RAISE_APPLICATION_ERROR</font> cu coduri in intervalul "
    "<font face='Courier'>-20001 .. -20099</font>, "
    "returnand un mesaj descriptiv catre aplicatie.",
    bg=LIGHT_OR, border=ORANGE
))

story.append(sp(0.5))
story.append(hr(GREY, 0.5))
story.append(p("Galeria de Arta — Proiect MODBD IFR 2025-2026 | "
               "Facultatea de Matematica si Informatica",
               ParagraphStyle("footer", parent=NR, fontSize=7,
                              textColor=GREY, alignment=TA_CENTER)))

# ── BUILD PDF ─────────────────────────────────────────────────────────────────
doc = SimpleDocTemplate(
    OUT,
    pagesize=A4,
    rightMargin=1.8*cm,
    leftMargin=1.8*cm,
    topMargin=1.8*cm,
    bottomMargin=1.8*cm,
    title="Cerinta 8 - Constrangeri de integritate - ArtGallery MODBD",
    author="ArtGallery MODBD",
)
doc.build(story)
print(f"PDF generat: {OUT}")
