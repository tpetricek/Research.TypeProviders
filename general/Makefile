default: providers

clean:
	rm -f *._tex
	rm -f *.aux
	rm -f *.bbl
	rm -f *.blg
	rm -f *.dvi
	rm -f *.pdf
	rm -f *.log
	rm -f *.ptb

providers: 
	latex -interaction=nonstopmode providers.tex
	mv providers.aux providers.bib.aux
	bibtex providers.bib
	mv providers.bib.bbl providers.bbl
	pdflatex -interaction=nonstopmode providers.tex
