## Top 3

> (1) Address the technical issue pointed out by reviewer E (in Figure 2)

We changed the Figure 2 so that it defines a function `lub` rather than
defining a relation and then (laborously) proving that it is a function.
This is shorter and more direct definition. We dropped the previous Lemma 1 
(lub is a function) and adapted Lemma 2 (it, indeed, defines least upper 
bound). We corrected the error pointed out by reviewer E (the case for 
handling nullables now results in `\ceil{lub(\sigma1, \sigma2)}`).

> (2) Substantiate the claim that the inference is predictable / stable



A full treatment of issue (2) would likely require more space than
available. However, we would like to add a brief section that makes the
following point: When a new sample is added to an existing set of samples,
the inferred shape can change only in certain limited ways (i.e. certain
parts may become optional). The fact that this change of shape is limited
shows that the inference algorithm is "stable" (users only need to do small
changes in their code). This is also relevant for the practical problem of
"choosing a representative sample". If an initial sample is not
representative and is refined later, the user will see which inferred types
became optional - and can adjust the program accordingly.

3) Add clarification of how XML is handled & discuss mixed-content XML
nodes

To address the concerns related to XML, we will add an expanded explanation
based on the one in the author response (also including a discussion of
mixed-content nodes as suggested by the additional comment from the
reviewer).



## Other random issues

> The relationship with the previous technical communications on 
> type providers is not entirely clear, but it may improve after 
> the double-blind restriction is lifted.

Clarified in Section 7 (ideas behind F# data date to the same time
as the development of F# type providers)

> p. 10, the reference for standard ML is broken

Fixed.

## Minor

> sec 1, para 2: eample --> example

Fixed.

> tried to type in the url for the example in the intro:
> http://api.owm.org/?q=NYC but it was not a valid url ...
> I notice that the authors seem to go back and forth between
> http://api.owm.org and http://openweathermap.org

Added a footnote (api.owm.org is an abbreviation used for
space and formatting reasons.)


> pg 3: "the the field" --> the field

Fixed.

> fig 2, any rule -- missing parens above the line, tagof(\sigma_2)

Deleted as part of another change.

> fig 2, record rule -- by using v1', ..., vm', you suggest that the field labels 
> in the 2nd record may be different from the field labels in the first record.
>  But I think you intend them to be same set of labels. 

This was an error, thanks! Corrected in the new formalization.

> a very pendantic notational comment (sorry): fig 7, rule (eq2) you write:
> v = v' \rigtharrow false; typically, v and v' are interpreted as meta-variables that may 
> range over any value in the grammar of that sort. In particular, v and v' may be 
> the same value, in which case v = v' would evaluate to true. 

Clarified (moved v \neq v' or v = v' into a meta-language side-condition)

> pg 8, top of left column, the authors write:
> [[ - ]] : \sigma -> (\tau  e  L)
> Throughout the rest of the paper \sigma, \tau, e, L are interpreted as meta-variables, 
> not types but here they are being used as types. To give a schema for the denotational 
> semantics in a more standard way, I would write: [[ \sigma ]] = (\tau, e, L)

Changed.

> consider replacing reference [8] with the following more complete work:
> From Dirt to Shovels: Fully Automatic Tool Generation From Ad Hoc Data. (...) POPL 2008.

Replaced.

> 2.2: The discussion of the open world and the type Element was confusing. 
> I thought an open world would mean that you don't assume that documents only have 
> heading, p, and image tags but also allow them to have other tags.

This was not clear - `Element` can represent other tags.
Added a brief note to clarify this.

> The choice of representing the "disjoint union" (...) Another choice, for 
> example, would be to have a class for each of them, and make all of those 
> classes be subclasses of a top Element class.

Added a note that this might be preferred in OO languages
(Although the nice '.' discoverability would be lost.)


> 2.3: It would be useful to show the shape you would infer for the example here.

Added









Def 1: This looks like a pretty standard subtyping relationship -- what is particularly interesting?

Fig 2, rule record: Should k be n?

3.5: The notion of labelled top shapes was a bit odd to me. It seems like a more general way to handle this issue is to explicitly support (some forms of) unions of shapes as shapes. Why this secondary add-on approach?

End of 3: "there are labels that enable typed access to every possible value in the sample, but not more." It would be useful to give this intuition earlier, and actually to state it as an explicit, ideally formalized goal. The "not more" part seems like the most interesting, since you are doing joins up the lattice, so it's hard to define what that means, let alone argue that you satisfy it.

Fig 6: This was a bit too abstract because I didn't know what the function arguments are and how they are produced (until it was described later).

Fig 8: It would help to show the op rules, since those are the new ones here -- most of what's in this figure is standard.

5: "This is an obligation upon the author of a type provider..." I'm confused -- is Lemma 3 contingent on some assumption about the type provider code, and if so, what is that assumption?

Theorem 4: I don't understand the e[y <- e' d'] part. You are treating e' as a function but it has type tau'.




p. 1: How dynamic are type providers, in practice? The paper's formalism generates the classes in two phases, implying a compile-time/run-time split, but in practice it seems that it could be messier; I would have liked a more direct treatment of type providers and/or more explanation of their limitations/assumptions.

−
> p. 2: "what code should be executed at run-time in place 
> of item.Name and other operations" -- I don't get this. 

Changed (this was very cumbersome)


> p. 3: I found it odd that 2.3 is a "summary" but then it introduces another example, 
> and forward references a feature not actually mentioned in section 2.

Renamed section.





−
p. 4: You don't specifically reference figure 2 when you first start talking about it, i.e., at the start of section 3.3.

−
p. 5: Figure 4 is referenced (on this page) before Fig. 3 (which appears along with its referencing text on the following page). Reorder.

−
p. 5: In 3.5 you mention associating a \rho variable with a record, but the grammar for d in 3.4 does not show it; it should be added there. You can/should also be clearer that \theta is a global substitution, and say more about the sense in which it is "minimal". It's not clear what constraints are contributing to \rho when reading in the data, so it's not clear how to solve them minimally. This part of the process is the least explained/most vague.

−
p. 6: In Fig. 3 you reference option<\sigma> -- should be nullable<\sigma>.

−
p. 9: The adjective "relativized" is stilted, to me. Why is "relative safety" not the right phrase, rather than "relativized safety"?

−
p. 9: The syntax (\lambda x \rightarrow e) for functions does not match the previously introduced syntax in Fig. 5 (\lambda x.e); I prefer the standard syntax (without the arrow). Also, in Fig. 9, the rule for null and \bot generates a function that calls the class C's constructor with no argument, but the generated L defines C as having an argument.

−
p. 9: The bullet list at the start of section 5 seems intuitive except for "Records in the input can have fewer fields, provided that the type of the field is nullable in some of the samples." But to have inferred a nullable field we would have seen a sample that was missing one; so I don't see in what sense the records would have "fewer fields". Indeed, Lemma 3 implies this when it says the records must have inferred sub-shapes compared to the ones from which the parser was generated. I would drop this bullet.

−
p. 10: Dangling reference after "(standard) ML".




> (...) capitalization of field names changes

Added clarification in the "Implementation" section 
with a forward reference in Section 2.



> (...) could use soe extra reminders about the details of the F# libraries. 
> The behavior of Option.iter deserves a more complete explanation

Added.




There is no explanation for why null should be treated as an empty collection but not as, say, an empty record. A design choice was made here, and the rationale for it is not obvious and deserves explanation.




(Other problems with Figure 2 include: (a) missing definitions for tagof(null) and tagof(\bottom); (b) inappropriate "arguments" to "any" in the definition of "tagof(any)"; (c) missing parentheses in "tagof \sigma_2" in rule (any); (d) inappropriate "chaining" of the \neq relation in one premise of rule (any).)

I must say that I do like your reuse of the floor and ceiling notation to indicate removal and introduction of "nullable". Very clever.

On page 11, it speaks of giving the body of an XML record the special field name \bullet, at least for the purposes on the calculus, but there is no indication of how an F# programmer is to refer to that field from within code.


> page 1: last sentence in column1 says that the example will "get the current 
> weather" but in fact it gets only the temperature. Using this more accurate 
> term "temperature" might help the poor reader to guess that "temp" in the 
> example code abbreviates "temperature" rather than "temporary"!

The record contains other fields but we added a note saying "the `temp` field
representing the current temperature" to clarify what `temp` stands for.


> page 1: The name "F# Data" is not mentioned in the abstract and appears 
> out of the blue, without explanation, in column 2.

Added mention into the abstract.

> page 4, figure 1: (...) two question marks missing (...) Also, WHY OH WHY 
> did you orient the figures so as to put "bottom" at the TOP of the figure?

Question marks added, figure flipped.

> page 6: the section title for Section 4 has "Formalising" (with an "s") 
> but the abstract uses the spelling "formalization" (with a "z")

Americanized for consistency.




## Unanswered

page 4: on a related note, why the chiasmus in the second line of Definition 1? First you mention \sigma_1 and \sigma_2 in that order, but then a few words later have to mention \sigma_2 before \sigma_1. And the need for this chiasmus arises ONLY because you alreayd made the unusual choice of using \sqsupset for your relational symbol rather than the more conventional \sqsubset (with arguments reverse). This is just poor choice of, or failure to pay attention to, notation and how it helps or hinders the reader.

Bibliography: Capitalization is incorrect or inconsistent in many entries. Examples: In the italicized book title of [1], all words except "of" should be capitalized. In the title of [4] "json" should be "JSON". In the title of [12], "ide" should be "IDE". In [16], the word "Of" should NOT be capitalized. The journal "SIGPLAN Notices" is mentioned in [7] as "Sigplan Notices", in [11] and [21] as "SIGPLAN Notices", and in [24] as "SIGPLAN Not." These are only a few examples; you need to double-check EVERY bibliographc entry.

ALWAYS inspect any BibTeX data downloaded from the Internet and edit it to conform to your chosen consistent style. Do NOT trust BibTeX data downloaded from the ACM Digital Library or other online resources; it is a good starting point, but must always be verified and usually edited to remove small problems. It is your responsibility to ensure a correct and consistent bibliographic style AND to verify the bibliographic data against the actual original paper.

−Does the library handle variations in the data format as type errors, or is there a way to capture choices using the shape inference algorithm?
−What do you think it would take to generalize type providers the way PADS has? 
−Did you observe any bottlenecks for the shape inference algorithm?


page 2, column 2, second paragraph: This immediately suggests a problem not addressed by the paper: providing good examples from which all relevant properties, and no more, may be easily inferred is a difficult art in itself at which many people are surprisingly unskilled. Do we have any assurance that a sample of data provided by an automated source such as openweathermap.org will in fact be sufficiently rich to serve as a proper example from which to infer the correct type for processing other, related data? What are the fallback options if this fails?
