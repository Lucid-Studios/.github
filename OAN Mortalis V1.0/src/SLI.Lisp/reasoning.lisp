(in-package :sli-core)

(defun decision-branch (&rest options)
  (list :branch options))

(defun cleave (branches)
  (list :cleave branches))

(defun commit (decision)
  (list :commit decision))
