#!/usr/bin/env python3
"""
Campaign Document Extractor for SCRIBE
Extracts text, color metadata, and structures content for RAG embedding.

⚠️ DEPRECATED: This functionality has been integrated into .NET DocumentParserService.
   The C# implementation in DA_Scribe/Services/DocumentParserService.cs now handles
   color detection and character mapping natively when processing Word documents.
   
   This script is kept for reference and manual batch processing outside the app.

Usage:
    python extract_campaign.py --input "../Kraina możliwości" --output "../processed"
"""

import os
import sys
import json
import re
import argparse
from pathlib import Path
from zipfile import ZipFile
from xml.etree import ElementTree as ET
from dataclasses import dataclass, field, asdict
from typing import List, Dict, Optional, Set
from collections import defaultdict
from datetime import datetime

# Word XML namespace
NS = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

# ============================================================================
# CHARACTER COLOR MAPPING
# ============================================================================
CHARACTER_COLORS = {
    'b45f06': 'Udar',
    'ff9900': 'Udar',
    '38761d': 'Tomin',
    '274e13': 'Tomin',
    '0000ff': 'Granit',
    '4a86e8': 'Granit',
    'ff0000': 'Glorio',
    'c27ba0': 'Bjorn',
    'a64d79': 'Bjorn',
    '980000': 'Sharu',
    '990000': 'Sharu',
    'cc0000': 'Sharu',
    '5b0f00': 'Sharu',
    '6fa8dc': 'Sharu',
    '660000': 'Sir Cedrick',
}

ARCHIVED_CHARACTERS = {'Sharu'}

IGNORED_COLORS = {
    '0000ee', '000000', '1155cc', 'auto',
    '674ea7', '351c75', '20124d',  # Baron Mevir
    'b7b7b7', '6aa84f', '1c4587', '434343',
    'e69138', '783f04', '434343', '85200c', 'a61c00',
    '666666', '999999', 'd5a6bd', '741b47', 'e06666',
}

# ============================================================================
# DATA CLASSES
# ============================================================================

@dataclass
class TextRun:
    """A single run of text with formatting metadata."""
    text: str
    color: Optional[str] = None
    character: Optional[str] = None
    is_bold: bool = False
    is_italic: bool = False

@dataclass
class Paragraph:
    """A paragraph with all its text runs."""
    runs: List[TextRun] = field(default_factory=list)
    is_dialogue: bool = False
    is_game_mechanic: bool = False
    pov_character: Optional[str] = None
    
    @property
    def full_text(self) -> str:
        return ''.join(run.text for run in self.runs)
    
    @property
    def characters_mentioned(self) -> Set[str]:
        chars = set()
        for run in self.runs:
            if run.character:
                chars.add(run.character)
        return chars

@dataclass
class Chunk:
    """A semantic chunk of the adventure ready for embedding."""
    id: str
    document_path: str
    act_number: Optional[str] = None
    scene_title: Optional[str] = None
    content: str = ""
    content_plain: str = ""  # Without color annotations
    pov_characters: List[str] = field(default_factory=list)
    characters_present: List[str] = field(default_factory=list)
    npcs_mentioned: List[str] = field(default_factory=list)
    locations: List[str] = field(default_factory=list)
    items_mentioned: List[str] = field(default_factory=list)
    date_in_game: Optional[str] = None
    has_dialogue: bool = False
    has_combat: bool = False
    has_game_mechanics: bool = False
    chunk_type: str = "narrative"  # narrative, dialogue, combat, mechanics, summary
    word_count: int = 0
    
@dataclass
class ProcessedDocument:
    """A fully processed document with all its chunks."""
    path: str
    filename: str
    document_type: str  # adventure, character, world, rules
    act_number: Optional[str] = None
    title: Optional[str] = None
    chunks: List[Chunk] = field(default_factory=list)
    all_characters: List[str] = field(default_factory=list)
    all_npcs: List[str] = field(default_factory=list)
    all_locations: List[str] = field(default_factory=list)
    date_range: Optional[str] = None
    summary: Optional[str] = None

# ============================================================================
# DOCUMENT PARSING
# ============================================================================

def extract_paragraphs(docx_path: str) -> List[Paragraph]:
    """Extract all paragraphs with color and formatting metadata from a Word document."""
    paragraphs = []
    
    try:
        with ZipFile(docx_path, 'r') as z:
            xml_content = z.read('word/document.xml')
            root = ET.fromstring(xml_content)
            
            for p_elem in root.findall('.//w:p', NS):
                para = Paragraph()
                
                for run in p_elem.findall('.//w:r', NS):
                    # Get text
                    text = ''.join(t.text or '' for t in run.findall('.//w:t', NS))
                    if not text:
                        continue
                    
                    # Get formatting
                    color = None
                    is_bold = False
                    is_italic = False
                    
                    rPr = run.find('w:rPr', NS)
                    if rPr is not None:
                        color_el = rPr.find('w:color', NS)
                        if color_el is not None:
                            color = color_el.get(f'{{{NS["w"]}}}val')
                        
                        if rPr.find('w:b', NS) is not None:
                            is_bold = True
                        if rPr.find('w:i', NS) is not None:
                            is_italic = True
                    
                    # Map color to character
                    character = None
                    if color and color.lower() not in IGNORED_COLORS:
                        character = CHARACTER_COLORS.get(color.lower())
                    
                    text_run = TextRun(
                        text=text,
                        color=color,
                        character=character,
                        is_bold=is_bold,
                        is_italic=is_italic
                    )
                    para.runs.append(text_run)
                
                if para.runs:
                    full_text = para.full_text
                    
                    # Detect dialogue (starts with dash)
                    para.is_dialogue = full_text.strip().startswith('-') or full_text.strip().startswith('–')
                    
                    # Detect game mechanics (parentheses with test/roll keywords)
                    para.is_game_mechanic = bool(re.search(
                        r'\((?:test|rzut|trafienie|obrażenia|inicjatywa|spostrzegawczość|siła|zręczność|wytrzymałość|inteligencja|mądrość|charyzma|atletyka|akrobatyka|percepcja|skradanie|perswazja|zastraszanie|oszustwo|wnikliwość|natura|religia|medycyna|przetrwanie|historia|arkana|vs|sprawność|biegłość|mod|bonus|kość|k\d+|d\d+)',
                        full_text, re.IGNORECASE
                    ))
                    
                    # Determine POV character (most prevalent color in paragraph)
                    char_counts = defaultdict(int)
                    for run in para.runs:
                        if run.character:
                            char_counts[run.character] += len(run.text)
                    if char_counts:
                        para.pov_character = max(char_counts, key=char_counts.get)
                    
                    paragraphs.append(para)
    
    except Exception as e:
        print(f"Error processing {docx_path}: {e}", file=sys.stderr)
    
    return paragraphs

# ============================================================================
# NPC AND LOCATION DETECTION
# ============================================================================

# Common location indicators in Polish
LOCATION_PATTERNS = [
    r'(?:w|na|do|z|przy|pod|przed|za|obok)\s+([A-ZŻŹĆĄĘÓŁŃ][a-zżźćąęółń]+(?:\s+[A-ZŻŹĆĄĘÓŁŃ][a-zżźćąęółń]+)*)',
    r'(?:karczma|gospoda|tawerna|zamek|wieża|świątynia|katedra|klasztor|warsztat|dom|pałac|arena|rynek|plac|dzielnica|ulica|most|brama|port|przystań|las|puszcza|góry|dolina|jaskinia|lochy|podziemia)\s+([A-ZŻŹĆĄĘÓŁŃ][^\s,\.]+(?:\s+[A-ZŻŹĆĄĘÓŁŃ][^\s,\.]+)*)',
]

# Known locations from the campaign
KNOWN_LOCATIONS = {
    'Pijany Smok', 'Warrington', 'Kojec', 'Zamtuz', 'Skundlona Arena',
    'Dębowy Pagórek', 'Klasztor Irori', 'Czerwony Bęben', 'Wesoły Partianin',
    'Stary Młyn', 'Zakon Gromu', 'Wschodnia Marchia', 'Dagareth', 'Mitholew',
    'Rynek', 'Plac Targowy', 'Warsztat lady Emely', 'Zamek Margrabiego',
}

# Title patterns for NPCs
NPC_TITLE_PATTERNS = [
    r'(?:pan|pani|lord|lady|sir|mistrz|mistrzyni|kapłan|kapłanka|książę|księżna|baron|baronowa|hrabia|hrabina|margrabia|markiza|diakon|starszy brat|brat|siostra|doktor|mag|czarodziej|czarodziejka)\s+([A-ZŻŹĆĄĘÓŁŃ][a-zżźćąęółń]+(?:\s+[A-ZŻŹĆĄĘÓŁŃ][a-zżźćąęółń]+)?)',
]

def extract_npcs(text: str, pcs: Set[str]) -> List[str]:
    """Extract NPCs mentioned in text (excluding PCs)."""
    npcs = set()
    
    # Find titled characters
    for pattern in NPC_TITLE_PATTERNS:
        for match in re.finditer(pattern, text, re.IGNORECASE):
            name = match.group(1).strip()
            if name and name not in pcs and len(name) > 2:
                # Include the title
                full_match = match.group(0).strip()
                npcs.add(full_match)
    
    return list(npcs)

def extract_locations(text: str) -> List[str]:
    """Extract locations mentioned in text."""
    locations = set()
    
    # Check known locations first
    for loc in KNOWN_LOCATIONS:
        if loc.lower() in text.lower():
            locations.add(loc)
    
    # Try patterns
    for pattern in LOCATION_PATTERNS:
        for match in re.finditer(pattern, text, re.IGNORECASE):
            loc = match.group(1).strip() if match.lastindex else match.group(0).strip()
            if loc and len(loc) > 2:
                # Filter out common words
                if loc.lower() not in {'ten', 'ta', 'to', 'się', 'być', 'mieć', 'który', 'która'}:
                    locations.add(loc)
    
    return list(locations)

def extract_date(text: str) -> Optional[str]:
    """Extract in-game date from text."""
    # Pattern: "X Month" or "X Month, ..."
    date_pattern = r'(\d{1,2})\s+(Erastus|Serenith|Abadius|Calistril|Pharast|Gozran|Desnus|Sarenith|Arodus|Rova|Lamashan|Neth|Kuthona)'
    match = re.search(date_pattern, text, re.IGNORECASE)
    if match:
        return f"{match.group(1)} {match.group(2)}"
    return None

def extract_items(text: str) -> List[str]:
    """Extract mentioned items, equipment, or possessions."""
    items = set()
    
    # Item patterns
    item_patterns = [
        r'(?:miecz|topór|buława|młot|włócznia|sztylet|łuk|kusza|zbroja|tarcza|hełm|rękawice|buty|płaszcz|pierścień|amulet|różdżka|laska|księga|zwój|mikstura|eliksir|trucizna|klucz|mapa|kompas|sakiewka|worek|plecak|torba)\s*(?:[a-zżźćąęółń]+)?',
        r'(?:złoty|srebrny|miedziany|platynowy)\s+(?:moneta|sztuka|złota|srebra)',
    ]
    
    for pattern in item_patterns:
        for match in re.finditer(pattern, text, re.IGNORECASE):
            items.add(match.group(0).strip())
    
    return list(items)

# ============================================================================
# CHUNKING LOGIC
# ============================================================================

def create_chunks(paragraphs: List[Paragraph], doc_path: str, max_words: int = 500) -> List[Chunk]:
    """
    Create semantic chunks from paragraphs.
    Chunks are split on:
    - POV character changes
    - Scene breaks (blank lines or headers)
    - Maximum word count
    """
    chunks = []
    current_chunk_paras = []
    current_pov = None
    current_word_count = 0
    chunk_counter = 0
    
    # Extract act number from filename
    act_match = re.search(r'[Aa]kt\s*(\d+(?:\.\d+)?)', doc_path)
    act_number = act_match.group(1) if act_match else None
    
    def finalize_chunk():
        nonlocal chunk_counter, current_chunk_paras, current_word_count, current_pov
        
        if not current_chunk_paras:
            return
        
        chunk_counter += 1
        
        # Combine paragraph texts
        content_parts = []
        content_plain_parts = []
        all_chars = set()
        has_dialogue = False
        has_mechanics = False
        
        for para in current_chunk_paras:
            # Annotated content (with character markers)
            annotated = []
            for run in para.runs:
                if run.character:
                    annotated.append(f"[{run.character}]{run.text}")
                    all_chars.add(run.character)
                else:
                    annotated.append(run.text)
            content_parts.append(''.join(annotated))
            content_plain_parts.append(para.full_text)
            
            if para.is_dialogue:
                has_dialogue = True
            if para.is_game_mechanic:
                has_mechanics = True
        
        content = '\n'.join(content_parts)
        content_plain = '\n'.join(content_plain_parts)
        
        # Determine POV characters
        pov_chars = list(set(p.pov_character for p in current_chunk_paras if p.pov_character))
        
        # Extract metadata
        npcs = extract_npcs(content_plain, all_chars)
        locations = extract_locations(content_plain)
        date = extract_date(content_plain)
        items = extract_items(content_plain)
        
        # Detect combat
        has_combat = bool(re.search(
            r'(?:atak|obrażenia|trafienie|chybienie|zranienie|rana|krew|walka|bitwa|starcie|unik|parowanie|blok)',
            content_plain, re.IGNORECASE
        ))
        
        # Determine chunk type
        chunk_type = "narrative"
        if has_combat:
            chunk_type = "combat"
        elif has_mechanics:
            chunk_type = "mechanics"
        elif has_dialogue and not has_mechanics:
            chunk_type = "dialogue"
        
        chunk = Chunk(
            id=f"{Path(doc_path).stem}_chunk_{chunk_counter:03d}",
            document_path=doc_path,
            act_number=act_number,
            content=content,
            content_plain=content_plain,
            pov_characters=pov_chars,
            characters_present=list(all_chars),
            npcs_mentioned=npcs,
            locations=locations,
            items_mentioned=items,
            date_in_game=date,
            has_dialogue=has_dialogue,
            has_combat=has_combat,
            has_game_mechanics=has_mechanics,
            chunk_type=chunk_type,
            word_count=len(content_plain.split())
        )
        chunks.append(chunk)
        
        current_chunk_paras = []
        current_word_count = 0
        current_pov = None
    
    for para in paragraphs:
        para_words = len(para.full_text.split())
        
        # Check if we should start a new chunk
        should_split = False
        
        # POV change (significant character shift)
        if para.pov_character and current_pov and para.pov_character != current_pov:
            should_split = True
        
        # Word count exceeded
        if current_word_count + para_words > max_words and current_chunk_paras:
            should_split = True
        
        if should_split:
            finalize_chunk()
        
        current_chunk_paras.append(para)
        current_word_count += para_words
        if para.pov_character:
            current_pov = para.pov_character
    
    # Finalize last chunk
    finalize_chunk()
    
    return chunks

# ============================================================================
# DOCUMENT TYPE DETECTION
# ============================================================================

def detect_document_type(path: str) -> str:
    """Detect the type of document based on path and name."""
    path_lower = path.lower()
    
    if 'zasady' in path_lower or 'walka' in path_lower or 'doświadczenie' in path_lower:
        return 'rules'
    elif 'opis świata' in path_lower or 'istotne elementy' in path_lower or 'geopolityczny' in path_lower:
        return 'world'
    elif any(char.lower() in path_lower for char in ['granit', 'tomin', 'udar', 'cedrick', 'bjorn', 'sharu', 'orion', 'roolf']):
        if 'akt' not in path_lower:
            return 'character'
    
    if 'akt' in path_lower or 'archiwum' in path_lower:
        return 'adventure'
    
    return 'other'

# ============================================================================
# MAIN PROCESSING
# ============================================================================

def process_document(docx_path: str) -> ProcessedDocument:
    """Process a single Word document and return structured data."""
    paragraphs = extract_paragraphs(docx_path)
    chunks = create_chunks(paragraphs, docx_path)
    
    # Aggregate metadata
    all_chars = set()
    all_npcs = set()
    all_locs = set()
    dates = set()
    
    for chunk in chunks:
        all_chars.update(chunk.characters_present)
        all_npcs.update(chunk.npcs_mentioned)
        all_locs.update(chunk.locations)
        if chunk.date_in_game:
            dates.add(chunk.date_in_game)
    
    # Extract act number and title from filename
    filename = Path(docx_path).stem
    act_match = re.search(r'[Aa]kt\s*(\d+(?:\.\d+)?)', filename)
    act_number = act_match.group(1) if act_match else None
    
    # Title is filename without act number prefix
    title = re.sub(r'^[Aa]kt\s*\d+(?:\.\d+)?\s*', '', filename).strip()
    if not title:
        title = filename
    
    return ProcessedDocument(
        path=docx_path,
        filename=filename,
        document_type=detect_document_type(docx_path),
        act_number=act_number,
        title=title,
        chunks=chunks,
        all_characters=list(all_chars),
        all_npcs=list(all_npcs),
        all_locations=list(all_locs),
        date_range=', '.join(sorted(dates)) if dates else None
    )

def process_campaign_folder(input_folder: str, output_folder: str):
    """Process all Word documents in a campaign folder."""
    input_path = Path(input_folder)
    output_path = Path(output_folder)
    output_path.mkdir(parents=True, exist_ok=True)
    
    # Find all docx files
    docx_files = list(input_path.rglob('*.docx'))
    print(f"Found {len(docx_files)} Word documents to process")
    
    all_documents = []
    all_chunks = []
    
    for i, docx_file in enumerate(sorted(docx_files), 1):
        print(f"[{i}/{len(docx_files)}] Processing: {docx_file.name}")
        
        try:
            doc = process_document(str(docx_file))
            all_documents.append(doc)
            all_chunks.extend(doc.chunks)
            
            # Save individual document JSON
            doc_output = output_path / f"{doc.filename}.json"
            with open(doc_output, 'w', encoding='utf-8') as f:
                json.dump(asdict(doc), f, ensure_ascii=False, indent=2)
        
        except Exception as e:
            print(f"  ERROR: {e}", file=sys.stderr)
    
    # Create summary statistics
    stats = {
        'total_documents': len(all_documents),
        'total_chunks': len(all_chunks),
        'documents_by_type': defaultdict(int),
        'all_characters': set(),
        'all_npcs': set(),
        'all_locations': set(),
        'chunks_by_type': defaultdict(int),
    }
    
    for doc in all_documents:
        stats['documents_by_type'][doc.document_type] += 1
        stats['all_characters'].update(doc.all_characters)
        stats['all_npcs'].update(doc.all_npcs)
        stats['all_locations'].update(doc.all_locations)
    
    for chunk in all_chunks:
        stats['chunks_by_type'][chunk.chunk_type] += 1
    
    # Convert sets to lists for JSON
    stats['all_characters'] = sorted(stats['all_characters'])
    stats['all_npcs'] = sorted(stats['all_npcs'])
    stats['all_locations'] = sorted(stats['all_locations'])
    stats['documents_by_type'] = dict(stats['documents_by_type'])
    stats['chunks_by_type'] = dict(stats['chunks_by_type'])
    
    # Save combined output
    combined_output = {
        'metadata': {
            'campaign': 'Kraina Możliwości',
            'processed_at': datetime.now().isoformat(),
            'stats': stats,
        },
        'chunks': [asdict(c) for c in all_chunks],
    }
    
    with open(output_path / 'all_chunks.json', 'w', encoding='utf-8') as f:
        json.dump(combined_output, f, ensure_ascii=False, indent=2)
    
    # Save stats summary
    with open(output_path / 'stats.json', 'w', encoding='utf-8') as f:
        json.dump(stats, f, ensure_ascii=False, indent=2)
    
    print(f"\n{'='*60}")
    print(f"Processing complete!")
    print(f"  Documents: {stats['total_documents']}")
    print(f"  Chunks: {stats['total_chunks']}")
    print(f"  Characters: {len(stats['all_characters'])}")
    print(f"  NPCs: {len(stats['all_npcs'])}")
    print(f"  Locations: {len(stats['all_locations'])}")
    print(f"\nOutput saved to: {output_path}")
    print(f"  - all_chunks.json (for SCRIBE import)")
    print(f"  - stats.json (summary)")
    print(f"  - Individual document JSONs")

def main():
    parser = argparse.ArgumentParser(description='Extract and process campaign documents for SCRIBE')
    parser.add_argument('--input', '-i', default='../Kraina możliwości/Kraina możliwości',
                        help='Input folder containing Word documents')
    parser.add_argument('--output', '-o', default='../processed',
                        help='Output folder for JSON files')
    
    args = parser.parse_args()
    
    # Resolve paths relative to script location
    script_dir = Path(__file__).parent
    input_path = (script_dir / args.input).resolve()
    output_path = (script_dir / args.output).resolve()
    
    if not input_path.exists():
        print(f"Error: Input folder not found: {input_path}", file=sys.stderr)
        sys.exit(1)
    
    process_campaign_folder(str(input_path), str(output_path))

if __name__ == '__main__':
    main()
