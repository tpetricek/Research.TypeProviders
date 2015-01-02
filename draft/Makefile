default: report

clean:
	rm -f *._tex
	rm -f *.aux
	rm -f *.bbl
	rm -f *.blg
	rm -f *.dvi
	rm -f *.pdf
	rm -f *.log
	rm -f *.ptb

report: clean
	latex -interaction=nonstopmode report.tex
	mv report.aux report.bib.aux
	bibtex report.bib
	mv report.bib.bbl report.bbl
	pdflatex -interaction=nonstopmode report.tex
