#!/usr/bin/env python3
"""
Extract prioritized backlog items from a markdown backlog and print them to the console.

Usage:
    python extractFromBacklog.py backlog.md --NbElements 10
    python extractFromBacklog.py backlog.md --NbPoints 20

Rules:
    --NbElements  Extract the first N sorted backlog items.
    --NbPoints    Extract sorted backlog items until the total points reaches
                  or exceeds the requested point count.

Supported columns:
    Priority | Pts | Item | Notes
    Priority | Estimate Points | Item | Notes
    Importance Factor | Estimate Points | Item | Notes

Sorting:
    1. Higher numeric Priority first, when Priority is a number.
    2. Legacy named priorities: Blocking, Core, Polish.
    3. Original document order.

Notes:
    - The script supports standard markdown tables with separator rows.
    - The script also supports your current backlog format, where the header row
      is followed directly by data rows.
"""

from __future__ import annotations

import argparse
import re
from dataclasses import dataclass
from pathlib import Path


LEGACY_PRIORITY_RANK = {
    "blocking": 0,
    "prioritized": 0,

    "core": 1,
    "major": 1,
    "medium": 1,

    "polish": 2,
    "minor": 2,
    "low": 2,
}


@dataclass(frozen=True)
class BacklogItem:
    section: str
    priority: str
    priority_score: float
    points: float | None
    item: str
    notes: str
    source_order: int


def clean_markdown_cell(value: str) -> str:
    value = value.strip()
    value = re.sub(r"^\*\*(.*?)\*\*$", r"\1", value)
    value = re.sub(r"^__(.*?)__$", r"\1", value)
    return value.strip()


def normalize_column_name(value: str) -> str:
    return re.sub(r"\s+", " ", clean_markdown_cell(value)).strip().lower()


def split_markdown_row(line: str) -> list[str]:
    line = line.strip()

    if line.startswith("|"):
        line = line[1:]

    if line.endswith("|"):
        line = line[:-1]

    return [clean_markdown_cell(cell) for cell in line.split("|")]


def is_separator_row(line: str) -> bool:
    cells = split_markdown_row(line)

    return bool(cells) and all(
        re.fullmatch(r":?-{3,}:?", cell.strip())
        for cell in cells
    )


def parse_number(value: str | None) -> float | None:
    if value is None:
        return None

    value = clean_markdown_cell(value)

    if not value:
        return None

    match = re.search(r"-?\d+(?:[.,]\d+)?", value)

    if match is None:
        return None

    return float(match.group(0).replace(",", "."))


def get_priority_score(priority: str) -> float:
    numeric_priority = parse_number(priority)

    if numeric_priority is not None:
        # Higher numeric priority should come first.
        return -numeric_priority

    legacy_rank = LEGACY_PRIORITY_RANK.get(priority.strip().lower())

    if legacy_rank is not None:
        return float(legacy_rank)

    return 999_999.0


def is_table_header(line: str) -> bool:
    if "|" not in line:
        return False

    headers = [normalize_column_name(header) for header in split_markdown_row(line)]

    has_priority = "priority" in headers or "importance factor" in headers
    has_item = "item" in headers

    return has_priority and has_item


def parse_backlog(markdown_text: str) -> list[BacklogItem]:
    items: list[BacklogItem] = []
    current_section = ""
    source_order = 0
    lines = markdown_text.splitlines()
    line_index = 0

    while line_index < len(lines):
        line = lines[line_index].rstrip()

        heading = re.match(r"^(#{2,6})\s+(.+?)\s*$", line)
        if heading:
            current_section = heading.group(2).strip()
            line_index += 1
            continue

        if not is_table_header(line):
            line_index += 1
            continue

        headers = split_markdown_row(line)
        normalized_headers = [normalize_column_name(header) for header in headers]

        priority_column = None
        for candidate in ("priority", "importance factor"):
            if candidate in normalized_headers:
                priority_column = normalized_headers.index(candidate)
                break

        if priority_column is None or "item" not in normalized_headers:
            line_index += 1
            continue

        item_column = normalized_headers.index("item")

        points_column = None
        for candidate in ("pts", "points", "estimate points", "estimate point"):
            if candidate in normalized_headers:
                points_column = normalized_headers.index(candidate)
                break

        notes_column = (
            normalized_headers.index("notes")
            if "notes" in normalized_headers
            else None
        )

        line_index += 1

        # Support standard markdown tables.
        if line_index < len(lines) and is_separator_row(lines[line_index]):
            line_index += 1

        while line_index < len(lines):
            row_line = lines[line_index].strip()

            if not row_line:
                break

            if "|" not in row_line:
                break

            # Stop if another table header starts.
            if is_table_header(row_line):
                break

            row = split_markdown_row(row_line)

            if len(row) < len(headers):
                row += [""] * (len(headers) - len(row))

            priority = clean_markdown_cell(row[priority_column])
            item = clean_markdown_cell(row[item_column])

            points = (
                parse_number(row[points_column])
                if points_column is not None
                else None
            )

            notes = (
                clean_markdown_cell(row[notes_column])
                if notes_column is not None
                else ""
            )

            if priority and item:
                items.append(
                    BacklogItem(
                        section=current_section,
                        priority=priority,
                        priority_score=get_priority_score(priority),
                        points=points,
                        item=item,
                        notes=notes,
                        source_order=source_order,
                    )
                )
                source_order += 1

            line_index += 1

        continue

    return items


def sort_items(items: list[BacklogItem]) -> list[BacklogItem]:
    return sorted(
        items,
        key=lambda item: (
            item.priority_score,
            item.source_order,
        ),
    )


def select_by_number_of_elements(
    items: list[BacklogItem],
    number_of_elements: int,
) -> list[BacklogItem]:
    return items[:number_of_elements]


def select_by_number_of_points(
    items: list[BacklogItem],
    number_of_points: float,
) -> list[BacklogItem]:
    selected_items: list[BacklogItem] = []
    total_points = 0.0

    for item in items:
        selected_items.append(item)

        if item.points is not None:
            total_points += item.points

        if total_points >= number_of_points:
            break

    return selected_items


def escape_table_cell(value: str) -> str:
    return value.replace("|", r"\|").replace("\n", "<br>")


def format_points(value: float | None) -> str:
    if value is None:
        return ""

    return f"{value:g}"


def render_markdown(items: list[BacklogItem]) -> str:
    total_points = sum(
        item.points
        for item in items
        if item.points is not None
    )

    lines = [
        f"Extracted items: {len(items)}",
        f"Total points: {total_points:g}",
        "",
        "| Priority | Pts | Section | Item | Notes |",
        "|---:|---:|---|---|---|",
    ]

    for item in items:
        lines.append(
            "| "
            + " | ".join(
                [
                    escape_table_cell(item.priority),
                    escape_table_cell(format_points(item.points)),
                    escape_table_cell(item.section),
                    escape_table_cell(item.item),
                    escape_table_cell(item.notes),
                ]
            )
            + " |"
        )

    return "\n".join(lines)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Extract and sort backlog items from a markdown backlog."
    )

    parser.add_argument(
        "backlog_path",
        help="Path to the markdown backlog file.",
    )

    selection_group = parser.add_mutually_exclusive_group(required=True)

    selection_group.add_argument(
        "--NbElements",
        type=int,
        help="Number of backlog elements to extract.",
    )

    selection_group.add_argument(
        "--NbPoints",
        type=float,
        help="Number of points to extract. The script stops after reaching or overtaking this number.",
    )

    return parser.parse_args()


def main() -> int:
    args = parse_args()

    backlog_path = Path(args.backlog_path)

    if not backlog_path.exists():
        print(f"Input file not found: {backlog_path}")
        return 1

    markdown_text = backlog_path.read_text(encoding="utf-8")

    items = parse_backlog(markdown_text)
    sorted_items = sort_items(items)

    if args.NbElements is not None:
        selected_items = select_by_number_of_elements(
            sorted_items,
            args.NbElements,
        )
    else:
        selected_items = select_by_number_of_points(
            sorted_items,
            args.NbPoints,
        )

    print(render_markdown(selected_items))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())