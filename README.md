Tech stack: C# .Net (WinForms), Ghostscript, Tesseract OCR.
- Optimize Otsu binarization so it is easier for text extraction
- Use 2 threads to do OCR (can configure to use more threads if necessary)
- Tesseract 5 does not allow to get font type and font size so after extracting words, I combined them in line based on the average of their Y1 and Y2.
