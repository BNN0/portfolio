from __future__ import annotations

from typing import Any, Dict, List, Literal, Optional, Union

from pydantic import BaseModel, ConfigDict, Field, field_validator


class StrictBaseModel(BaseModel):
    model_config = ConfigDict(
        extra="forbid",
        populate_by_name=True,
        validate_assignment=True,
    )


class GenericEntity(StrictBaseModel):
    value: str
    page: Optional[int] = None
    context: Optional[str] = None


class DateEntity(StrictBaseModel):
    value: str
    normalized_value: Optional[str] = None
    page: Optional[int] = None
    context: Optional[str] = None


class AmountEntity(StrictBaseModel):
    value: str
    normalized_value: Optional[float] = None
    currency: Optional[str] = None
    page: Optional[int] = None
    context: Optional[str] = None


class IdentifierEntity(StrictBaseModel):
    value: str
    identifier_type: Optional[str] = None
    page: Optional[int] = None
    context: Optional[str] = None


class PhoneEntity(StrictBaseModel):
    value: str
    normalized_value: Optional[str] = None
    page: Optional[int] = None
    context: Optional[str] = None


class DocumentMetadata(StrictBaseModel):
    document_type: Optional[str] = None
    document_subtype: Optional[str] = None
    document_title: Optional[str] = None
    source_format: Optional[
        Literal["pdf", "image", "xlsx", "xls", "xml", "docx", "txt", "html", "unknown"]
    ] = None
    language: Optional[str] = None
    page_count: Optional[int] = None
    has_tables: bool = False
    has_images: bool = False
    has_handwriting: Optional[bool] = None
    issuer: Optional[str] = None
    recipient: Optional[str] = None
    extraction_timestamp: Optional[str] = None
    processor_version: Optional[str] = None


class Summary(StrictBaseModel):
    high_level_description: Optional[str] = None
    main_entities: List[str] = Field(default_factory=list)
    main_topics: List[str] = Field(default_factory=list)
    document_purpose: Optional[str] = None


class IndividualDataItem(StrictBaseModel):
    field_name: Optional[str] = None
    field_label: Optional[str] = None
    value: Optional[Union[str, int, float, bool]] = None
    normalized_value: Optional[Union[str, int, float, bool]] = None
    data_type: Optional[str] = None
    unit: Optional[str] = None
    currency: Optional[str] = None
    confidence: Optional[float] = None
    page: Optional[int] = None
    section: Optional[str] = None
    bounding_reference: Optional[str] = None
    source_snippet: Optional[str] = None

    @field_validator("confidence")
    @classmethod
    def validate_confidence(cls, v: Optional[float]):
        if v is not None and not (0 <= v <= 1):
            raise ValueError("confidence must be between 0 and 1")
        return v


class TableItem(StrictBaseModel):
    table_name: Optional[str] = None
    table_type: Optional[str] = None
    page: Optional[int] = None
    section: Optional[str] = None
    headers: List[str] = Field(default_factory=list)
    rows: List[List[Any]] = Field(default_factory=list)
    rows_as_objects: List[Dict[str, Any]] = Field(default_factory=list)
    row_count: int = 0
    column_count: int = 0
    table_notes: Optional[str] = None
    confidence: Optional[float] = None

    @field_validator("confidence", "row_count", "column_count")
    @classmethod
    def validate_positive(cls, v):
        if isinstance(v, float) and v is not None and not (0 <= v <= 1):
            raise ValueError("confidence must be between 0 and 1")
        if isinstance(v, int) and v < 0:
            raise ValueError("count must be >= 0")
        return v


class SectionItem(StrictBaseModel):
    section_name: Optional[str] = None
    page_start: Optional[int] = None
    page_end: Optional[int] = None
    raw_text: Optional[str] = None
    key_points: List[str] = Field(default_factory=list)
    confidence: Optional[float] = None


class Entities(StrictBaseModel):
    people: List[GenericEntity] = Field(default_factory=list)
    organizations: List[GenericEntity] = Field(default_factory=list)
    dates: List[DateEntity] = Field(default_factory=list)
    amounts: List[AmountEntity] = Field(default_factory=list)
    identifiers: List[IdentifierEntity] = Field(default_factory=list)
    addresses: List[GenericEntity] = Field(default_factory=list)
    emails: List[GenericEntity] = Field(default_factory=list)
    phones: List[PhoneEntity] = Field(default_factory=list)
    urls: List[GenericEntity] = Field(default_factory=list)


class TotalsAndCalculationsItem(StrictBaseModel):
    label: Optional[str] = None
    value: Optional[Union[str, int, float]] = None
    normalized_value: Optional[float] = None
    currency: Optional[str] = None
    formula_hint: Optional[str] = None
    page: Optional[int] = None
    section: Optional[str] = None
    source_snippet: Optional[str] = None
    confidence: Optional[float] = None


class WarningItem(StrictBaseModel):
    type: Optional[str] = None
    message: Optional[str] = None
    page: Optional[int] = None
    severity: Optional[Literal["low", "medium", "high"]] = None


class ExtractionQuality(StrictBaseModel):
    overall_confidence: Optional[float] = None
    ocr_issues_detected: bool = False
    missing_sections_detected: bool = False
    table_structure_uncertain: bool = False
    notes: List[str] = Field(default_factory=list)


class DocumentExtractionResponse(StrictBaseModel):
    document_metadata: DocumentMetadata = Field(default_factory=DocumentMetadata)
    summary: Summary = Field(default_factory=Summary)
    individual_data: List[IndividualDataItem] = Field(default_factory=list)
    tables: List[TableItem] = Field(default_factory=list)
    sections: List[SectionItem] = Field(default_factory=list)
    entities: Entities = Field(default_factory=Entities)
    totals_and_calculations: List[TotalsAndCalculationsItem] = Field(default_factory=list)
    warnings: List[WarningItem] = Field(default_factory=list)
    extraction_quality: ExtractionQuality = Field(default_factory=ExtractionQuality)


FULL_SYSTEM_PROMPT = """
You are a document information extraction engine.

Your only task is to extract information from the provided document content and return a STRICTLY VALID JSON object.

OUTPUT RULES:
- Return ONLY valid JSON.
- Do NOT return markdown.
- Do NOT wrap the JSON in ```json fences.
- Do NOT include explanations, notes, comments, or extra text.
- If data is missing, unreadable, or not present, use null.
- Never hallucinate values.
- Preserve original values when possible and also provide normalized values when applicable.
- Always include all root keys exactly as specified.
- Arrays must always be returned (even if empty).
- If no items are found for a section, return an empty array.
- Confidence values must be between 0 and 1 when possible.

EXTRACTION GOALS:
1. Extract all standalone fields as structured items in "individual_data".
2. Extract all tabular content in "tables".
3. Preserve table structure:
   - headers
   - rows as arrays
   - rows_as_objects using headers as keys
4. Extract relevant text blocks into "sections".
5. Identify entities such as people, organizations, dates, amounts, identifiers, addresses, emails, phones, and URLs.
6. Capture totals, subtotals, balances, taxes, calculations, and financial summary values in "totals_and_calculations".
7. Report OCR issues, ambiguities, or inconsistencies in "warnings".
8. Infer the likely document type if possible (invoice, statement, contract, form, receipt, report, purchase order, etc.).

JSON SCHEMA TO FOLLOW EXACTLY:

{
  "document_metadata": {
    "document_type": null,
    "document_subtype": null,
    "document_title": null,
    "source_format": null,
    "language": null,
    "page_count": null,
    "has_tables": false,
    "has_images": false,
    "has_handwriting": null,
    "issuer": null,
    "recipient": null,
    "extraction_timestamp": null,
    "processor_version": null
  },
  "summary": {
    "high_level_description": null,
    "main_entities": [],
    "main_topics": [],
    "document_purpose": null
  },
  "individual_data": [],
  "tables": [],
  "sections": [],
  "entities": {
    "people": [],
    "organizations": [],
    "dates": [],
    "amounts": [],
    "identifiers": [],
    "addresses": [],
    "emails": [],
    "phones": [],
    "urls": []
  },
  "totals_and_calculations": [],
  "warnings": [],
  "extraction_quality": {
    "overall_confidence": null,
    "ocr_issues_detected": false,
    "missing_sections_detected": false,
    "table_structure_uncertain": false,
    "notes": []
  }
}

NORMALIZATION: Dates to ISO, Amounts numeric, Phones clean, IDs preserve.
IMPORTANT: Standalone to individual_data, Tables to tables. No flattening.
""".strip()

