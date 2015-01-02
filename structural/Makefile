default: structural

clean:
	rm -f *._tex
	rm -f *.aux
	rm -f *.bbl
	rm -f *.blg
	rm -f *.dvi
	rm -f *.pdf
	rm -f *.log
	rm -f *.ptb

structural: 
	latex -interaction=nonstopmode structural.tex
	mv structural.aux structural.bib.aux
	bibtex structural.bib
	mv structural.bib.bbl structural.bbl
	pdflatex -interaction=nonstopmode structural.tex
