(in-package :sli-core)

(defun engram-ref (id)
  (list :engram id))

(defun context-expand (engram)
  (list :context engram))

(defun engram-query (concept)
  (list :query concept))
