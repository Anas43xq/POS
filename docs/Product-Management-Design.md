# Product Management – Finalized Design (Frozen)

## Main Layout

```
+------------------------------------------------------------------+
| Product Management                            [+] [✏] [🗑]        |
+---------------------------+--------------------------------------+
| Categories                | Products                             |
|---------------------------|--------------------------------------|
| 🔍 Search Category        | 🔍 Search Product                    |
|                           |                                      |
| ▼ Food                    | Product List                         |
|    Burgers                |                                      |
|    Sandwiches             |                                      |
|    Pizza                  |                                      |
|                           |                                      |
| ▼ Drinks                  |                                      |
|    Coffee                 |                                      |
|    Juice                  |                                      |
|                           |                                      |
| Desserts                  |                                      |
| Extras                    |                                      |
+---------------------------+--------------------------------------+
```

---

## Left Panel – Categories

- TreeView for categories.
- Supports unlimited subcategories.
- Categories without children appear as normal items.
- Category search filters the TreeView.
- Selecting a parent category displays all products in that category.
- Selecting a subcategory displays only products in that subcategory.

---

## Right Panel – Products

### Product Search

Search supports:

- Product Name
- Barcode

---

### Product List (DataGrid)

**Columns:**

| Column | Purpose |
|---|---|
| Product Name | Main product name |
| Category | Parent category |
| Subcategory | Product subcategory (blank if none) |
| Price | Selling price |
| Stock | Current quantity (or ∞ if stock is not tracked) |
| Status | Active / Inactive |

---

### Grid Behavior

- Search filters the product list.
- Click a column header to sort.
- Single-row selection.
- Double-click a row to edit the selected product.
- Left panel category selection filters the grid:
  - Parent category → all products in that category.
  - Subcategory → only products in that subcategory.
  - Root (All Categories) → all products.

---

## Toolbar

**Buttons:**

- ➕ Add
- ✏ Edit
- 🗑 Delete

Buttons are context-sensitive based on the current selection.

---

## Product Add/Edit Dialog

**Note:** Add/Edit is presented as a **popup (modal dialog)**, separate from the grid.

**Fields:**

- Product Name
- Parent Category (ComboBox)
- Subcategory (ComboBox)
  - Updates automatically when the Parent Category changes.
  - Disabled or shows (None) if the selected parent category has no subcategories.
- Price
- Barcode
- Status

---

## Design Status

This Product Management design is finalized and frozen for implementation.
